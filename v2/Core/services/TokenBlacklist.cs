public static class TokenBlacklist
{
    private static readonly HashSet<string> _revokedTokens = new();

    public static void Add(string jti)
    {
        _revokedTokens.Add(jti);
    }

    public static bool Contains(string jti)
    {
        return _revokedTokens.Contains(jti);
    }
}