using NewLife.Caching;

namespace Renova.Core.Components.Cache.Lock;

/// <summary>
/// 分布式锁扩展方法
/// </summary>
public static class DistributedLockExtensions
{
    /// <summary>
    /// 获取分布式锁（带重试机制）
    /// </summary>
    /// <param name="cache">缓存实例（FullRedis、MemoryCache）</param>
    /// <param name="name">锁名称</param>
    /// <param name="timeoutSeconds">锁超时时间（秒，默认30）</param>
    /// <param name="autoDelay">是否自动续期（默认true）</param>
    /// <returns>锁控制器或 null（超时未获取到锁）</returns>
    public static LockController? Lock(this ICache cache, string name, int timeoutSeconds = 30, bool autoDelay = true)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        name = "RedisClientLock:" + name;
        DateTime startTime = DateTime.Now;

        while (DateTime.Now.Subtract(startTime).TotalSeconds < timeoutSeconds)
        {
            var value = Guid.NewGuid().ToString("N");

            if (cache.Add(name, value, timeoutSeconds))
            {
                var refreshInterval = Math.Max(1.0, timeoutSeconds / 3.0);
                return new LockController(cache, name, value, timeoutSeconds, refreshInterval, autoDelay, CancellationToken.None);
            }

            //await Task.Delay(3);//延迟3毫秒重试
            Thread.Sleep(3);
        }

        return null;
    }


    /// <summary>
    /// 尝试获取分布式锁（单次尝试，无重试）
    /// </summary>
    /// <param name="cache">缓存实例（FullRedis、MemoryCache）</param>
    /// <param name="name">锁名称</param>
    /// <param name="timeoutSeconds">锁超时时间（秒，默认30）</param>
    /// <param name="autoDelay">是否自动续期（默认true）</param>
    /// <returns>锁控制器或 null（获取失败）</returns>
    public static LockController? TryLock(this ICache cache, string name, int timeoutSeconds = 30, bool autoDelay = true)
    {
        if (string.IsNullOrEmpty(name)) throw new ArgumentNullException(nameof(name));

        name = "RedisClientLock:" + name;
        var value = Guid.NewGuid().ToString("N");

        if (cache.Add(name, value, timeoutSeconds))
        {
            var refreshInterval = Math.Max(1.0, timeoutSeconds / 3.0);
            return new LockController(cache, name, value, timeoutSeconds, refreshInterval, autoDelay, CancellationToken.None);
        }

        return null;
    }
}

