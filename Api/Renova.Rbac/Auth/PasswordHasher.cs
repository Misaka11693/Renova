namespace Renova.Rbac.Auth;

/// <summary>
/// 统一处理 RBAC 密码哈希与校验。
/// </summary>
public static class PasswordHasher
{
    /// <summary>
    /// 对明文密码进行 MD5 哈希，返回小写字符串。
    /// </summary>
    public static string Hash(string value)
    {
        var bytes = MD5.HashData(Encoding.UTF8.GetBytes(value));
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    /// <summary>
    /// 校验输入密码是否与已存密码一致。
    /// </summary>
    public static bool Verify(string inputPassword, string storedPassword)
    {
        if (string.Equals(inputPassword, storedPassword, StringComparison.Ordinal))
        {
            return true;
        }

        return string.Equals(Hash(inputPassword), storedPassword, StringComparison.OrdinalIgnoreCase);
    }
}
