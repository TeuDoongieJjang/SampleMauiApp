namespace SampleMauiApp.Services;

public static class VersionHelper
{
    /// <summary>
    /// Returns true if <paramref name="candidate"/> is strictly greater than
    /// <paramref name="baseline"/>. Pre-release/build suffixes are stripped
    /// before comparison ("1.2.0-alpha.1" → "1.2.0").
    /// </summary>
    public static bool IsGreater(string? candidate, string? baseline)
    {
        var c = ParseSafe(candidate);
        var b = ParseSafe(baseline);
        if (c is null || b is null) return false;
        return c > b;
    }

    private static Version? ParseSafe(string? input)
    {
        if (string.IsNullOrWhiteSpace(input)) return null;

        // Strip pre-release ("-alpha.1") and build metadata ("+sha").
        var core = input.Split('-', '+')[0];

        // System.Version requires at least 2 parts; pad "1" → "1.0".
        if (core.Count(ch => ch == '.') == 0) core += ".0";

        return Version.TryParse(core, out var v) ? v : null;
    }
}