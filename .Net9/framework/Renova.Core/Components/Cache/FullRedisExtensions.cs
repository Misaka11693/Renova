namespace NewLife.Caching;

/// <summary>
/// FullRedis 的扩展方法，修复原生 Eval 的参数拼接问题。
/// </summary>
public static class FullRedisExtensions
{
    /// <summary>
    /// 正确执行 Lua 脚本（NewLife.Caching 中的 Eval 会因参数格式错误导致 Redis 报错）
    /// </summary>
    public static T? EvalFixed<T>(this FullRedis redis, string script, string[]? keys, object[]? args)
    {
        keys ??= [];
        args ??= [];

        var parameters = new List<object>
        {
            script,
            keys.Length 
        };

        parameters.AddRange(keys);
        parameters.AddRange(args);

        return redis.Execute("", (rc, k) => rc.Execute<T>("EVAL", parameters.ToArray()), write: true);
    }
}