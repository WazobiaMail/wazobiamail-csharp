namespace WazobiaMail.Http;

internal static class SecretMasker
{
    public static string Mask(string? secret)
    {
        if (string.IsNullOrWhiteSpace(secret))
        {
            return "***";
        }

        var trimmed = secret.Trim();
        if (trimmed.Length <= 8)
        {
            return "***";
        }

        return string.Concat(trimmed.AsSpan(0, 4), "...", trimmed.AsSpan(trimmed.Length - 4));
    }
}
