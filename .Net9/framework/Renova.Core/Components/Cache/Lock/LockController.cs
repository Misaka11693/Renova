using NewLife.Caching;

namespace Renova.Core.Components.Cache.Lock;

/// <summary>
/// 分布式锁控制器，支持自动续租
/// </summary>
public class LockController : IDisposable
{
    private readonly ICache _cache;                        // 底层缓存实例（Redis 或 MemoryCache 等）
    private readonly string _key;                          // 锁在缓存中的键名
    private readonly string _token;                        // 当前锁持有者的唯一标识（GUID）
    private readonly int _timeoutSeconds;                  // 锁的过期时间（秒）
    private readonly CancellationToken _cancellationToken; // 外部取消令牌，用于提前终止续租
    private readonly bool _autoDelay;                      // 是否启用自动续租（看门狗机制）
    private readonly FullRedis? _redis;                    // 仅当缓存为 FullRedis 时非空，用于执行 Lua 脚本
    private Timer? _autoDelayTimer;                        // 自动续租定时器，用于定期刷新锁的 TTL
    private bool _disposed;                                // 标记对象是否已被释放，防止重复释放

    #region Lua 脚本（仅 FullRedis 使用）

    /// <summary>
    /// 解锁 Lua 脚本：仅当当前持有者（value 匹配 _token）时才删除锁。
    /// 返回 1 表示成功删除，0 表示无权限或锁不存在。
    /// </summary>
    private static readonly string UnlockScript = @"
local current = redis.call('GET', KEYS[1])
if current == ARGV[1] then
    return redis.call('DEL', KEYS[1])
else
    return 0
end";

    /// <summary>
    /// 续租 Lua 脚本：仅当当前持有者（value 匹配 _token）时才重置锁的过期时间。
    /// ARGV[2] 为新的 TTL（毫秒）。返回 1 表示成功续租，0 表示锁已丢失。
    /// </summary>
    private static readonly string RenewScript = @"
local current = redis.call('GET', KEYS[1])
if current == ARGV[1] then
    return redis.call('PEXPIRE', KEYS[1], ARGV[2])
else
    return 0
end";

    #endregion

    /// <summary>
    /// 初始化锁控制器。
    /// </summary>
    /// <param name="cache">缓存实例，支持 FullRedis 或 MemoryCache 等</param>
    /// <param name="key">锁的键名</param>
    /// <param name="token">锁持有者唯一标识（由调用方生成）</param>
    /// <param name="timeoutSeconds">锁的过期时间（秒）</param>
    /// <param name="refreshIntervalSeconds">自动续租间隔（秒），建议为 timeoutSeconds 的 1/3</param>
    /// <param name="autoDelay">是否启用自动续租</param>
    /// <param name="cancellationToken">用于取消续租操作的令牌</param>
    internal LockController(
        ICache cache,
        string key,
        string token,
        int timeoutSeconds,
        double refreshIntervalSeconds,
        bool autoDelay,
        CancellationToken cancellationToken)
    {
        _cache = cache;
        _key = key;
        _token = token;
        _timeoutSeconds = timeoutSeconds;
        _cancellationToken = cancellationToken;
        _autoDelay = autoDelay;
        _redis = cache as FullRedis;

        if (_autoDelay)
        {
            var refreshIntervalMs = (int)(refreshIntervalSeconds * 1000);
            _autoDelayTimer = new Timer(_ => Refresh(), null, refreshIntervalMs, refreshIntervalMs);
        }
    }

    /// <summary>
    /// 执行续租操作：将锁的 TTL 重置为初始值（_timeoutSeconds）。
    /// - FullRedis：通过 Lua 脚本原子续租；
    /// - 其他缓存：通过 Get + Set 模拟续租（带 Token 校验，适用于单机场景）。
    /// 注意：此操作是重置 TTL，不是追加时间。例如原 TTL 剩 5 秒，续租后变为 _timeoutSeconds 秒。
    /// </summary>
    private void Refresh()
    {
        if (_disposed || _cancellationToken.IsCancellationRequested)
        {
            StopAutoDelay();
            return;
        }

        try
        {
            if (_redis != null)
            {
                var result = _redis.Eval<long>(RenewScript, [_key], [_token, _timeoutSeconds * 1000]);
                if (result != 1) StopAutoDelay();
            }
            else
            {
                var current = _cache.Get<string>(_key);
                if (current == _token)
                {
                    _cache.Set(_key, _token, _timeoutSeconds);
                }
                else
                {
                    StopAutoDelay();
                }
            }
        }
        catch
        {
            // 续租失败视为锁丢失，停止续租
            StopAutoDelay();
        }
    }

    /// <summary>
    /// 安全释放锁。
    /// - FullRedis：通过 Lua 脚本原子删除；
    /// - 其他缓存：先校验 Token 再删除。
    /// </summary>
    /// <returns>是否成功释放锁</returns>
    public bool Unlock()
    {
        if (_disposed) return false;
        _disposed = true;

        try
        {
            if (_redis != null)
            {
                var result = _redis.Eval<long>(UnlockScript, [_key], [_token]);
                return result == 1;
            }
            else
            {
                var current = _cache.Get<string>(_key);
                if (current == _token)
                {
                    return _cache.Remove(_key) == 1;
                }
                return false;
            }
        }
        catch
        {
            return false;
        }
        finally
        {
            StopAutoDelay();
        }
    }

    /// <summary>
    /// 手动重置锁的过期时间（非追加）。
    /// 注意：
    /// - 此方法会将锁的 TTL 设置为指定值，而非在当前 TTL 基础上延长。
    /// - 对于内存缓存，由于无法原子获取剩余 TTL，直接重置为新值（可能缩短总持有时间）。
    /// </summary>
    /// <param name="milliseconds">新的过期时间（毫秒）</param>
    /// <returns>是否成功重置</returns>
    public bool Delay(int milliseconds)
    {
        if (_disposed) return false;
        var seconds = Math.Max(1, (int)Math.Ceiling(milliseconds / 1000.0));

        try
        {
            if (_redis != null)
            {
                var result = _redis.Eval<long>(RenewScript, [_key], [_token, milliseconds]);
                return result == 1;
            }
            else
            {
                var current = _cache.Get<string>(_key);
                if (current == _token)
                {
                    _cache.Set(_key, _token, seconds);
                    return true;
                }
                return false;
            }
        }
        catch
        {
            return false; // 静默失败
        }
    }

    /// <summary>
    /// 停止自动续租定时器并释放资源。
    /// </summary>
    private void StopAutoDelay() 
    {
        _autoDelayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _autoDelayTimer?.Dispose();
    }

    /// <summary>
    /// 释放锁资源（调用 Unlock）。
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Unlock();
        }
    }
}