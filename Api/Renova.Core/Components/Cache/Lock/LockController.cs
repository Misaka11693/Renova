using NewLife.Caching;

namespace Renova.Core.Components.Cache.Lock;

/// <summary>
/// 缓存锁控制器，支持内存和分布式缓存，支持锁自动续期
/// </summary>
public class LockController : IDisposable
{
    private readonly ICache _cache;                        // 缓存实例（Redis 或 MemoryCache）
    private readonly string _key;                          // 锁在缓存中的键名
    private readonly string _token;                        // 当前锁持有者的唯一标识（GUID）
    private readonly int _timeoutSeconds;                  // 锁的过期时间（秒）
    private readonly CancellationToken _cancellationToken; // 外部取消令牌，用于提前终止续租
    private readonly bool _autoDelay;                      // 是否启用自动续租（看门狗机制）
    private readonly FullRedis? _redis;                    // 仅当缓存为 FullRedis 时非空，用于执行 Lua 脚本
    private Timer? _autoDelayTimer;                        // 自动续租定时器，用于定期刷新锁的过期时间
    private bool _disposed;                                // 标记对象是否已被释放，防止重复释放

    #region Lua 脚本（仅 FullRedis 使用）

    /// <summary>
    /// 解锁 Lua 脚本：仅当当前持有者（value 匹配 _token）时才删除锁。
    /// 返回 1 表示成功删除，0 表示无权限或锁不存在。
    /// </summary>
    private readonly string UnlockScript = @"
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
    private readonly string RenewScript = @"
local current = redis.call('GET', KEYS[1])
if current == ARGV[1] then
    return redis.call('PEXPIRE', KEYS[1], ARGV[2])
else
    return 0
end";

    #endregion

    /// <summary>
    /// 初始化缓存锁控制器
    /// </summary>
    /// <param name="cache">缓存实例，FullRedis 或 MemoryCache </param>
    /// <param name="key">锁的唯一键名</param>
    /// <param name="token">当前线程的唯一标识，用于锁所有权校验</param>
    /// <param name="timeoutSeconds">锁的过期时间（秒）</param>
    /// <param name="refreshIntervalSeconds">自动续租间隔（秒）</param>
    /// <param name="autoDelay">是否启用自动续期机制</param>
    /// <param name="cancellationToken">用于外部取消自动续期的令牌</param>
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
    /// 执行锁的续租：
    /// - FullRedis：通过 Lua 脚本原子续租
    /// - MemoryCache：通过 Get + Set 续租
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

                var result = _redis.EvalFixed<long>(RenewScript, [_key], [_token, _timeoutSeconds * 1000]); //Console.WriteLine($"{_token}:续租成功");
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
    /// 释放锁
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
                var result = _redis.EvalFixed<long>(UnlockScript, [_key], [_token]); //Console.WriteLine($"{_token}:释放成功");
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
    /// 重置锁的过期时间（非追加）
    /// 注意：
    /// - 此方法会将锁的 TTL 设置为指定值，而非在当前 TTL 基础上延长。
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
                var result = _redis.EvalFixed<long>(RenewScript, [_key], [_token, milliseconds]);
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
    /// 停止自动续租定时器并释放资源
    /// </summary>
    private void StopAutoDelay()
    {
        _autoDelayTimer?.Change(Timeout.Infinite, Timeout.Infinite);
        _autoDelayTimer?.Dispose();
    }

    /// <summary>
    /// 释放锁资源
    /// </summary>
    public void Dispose()
    {
        if (!_disposed)
        {
            Unlock();
        }
    }
}