namespace EventEase.Services;

/// <summary>Small text helpers shared by components.</summary>
public static class Text
{
    public static string FirstName(string? name) =>
        (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault() ?? string.Empty;

    public static string Initials(string? name)
    {
        var parts = (name ?? string.Empty).Split(' ', StringSplitOptions.RemoveEmptyEntries);
        return parts.Length switch
        {
            0 => "?",
            1 => parts[0][..1].ToUpperInvariant(),
            _ => string.Concat(parts[0][..1], parts[^1][..1]).ToUpperInvariant()
        };
    }

    /// <summary>Trims and bounds free-text input (e.g. search terms) to avoid oversized or whitespace-only queries.</summary>
    public static string Sanitize(string? input, int maxLength = 60)
    {
        var value = (input ?? string.Empty).Trim();
        return value.Length > maxLength ? value[..maxLength] : value;
    }

    /// <summary>Only allows app-relative return URLs to prevent open-redirect / broken routing.</summary>
    public static string? SafeLocalPath(string? url)
    {
        if (string.IsNullOrWhiteSpace(url)) return null;
        url = url.Trim();
        if (url.StartsWith("//") || url.StartsWith("/\\") || url.Contains(':')) return null;
        return Uri.TryCreate(url, UriKind.Relative, out _) ? url.TrimStart('/') : null;
    }
}
