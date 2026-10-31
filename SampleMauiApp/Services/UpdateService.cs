using SampleMauiApp.Models;
using System.Text.Json;

namespace SampleMauiApp.Services;

public class UpdateService
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        // GitHub redirects to a CDN; let HttpClient follow it automatically.
        Timeout = TimeSpan.FromSeconds(30)
    };

    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    /// <summary>
    /// Checks GitHub's latest release for an APK newer than the installed one.
    /// Returns null if no update is available (or if anything goes wrong).
    /// </summary>
    public async Task<UpdateInfo?> CheckForUpdateAsync(string repoOwner, string repoName)
    {
        try
        {
            // ── 1. Read local version info ────────────────────────────────
            var localVersionName = AppInfo.Current.VersionString;           // "1.2.0"
            var localVersionCode = ParseIntSafe(AppInfo.Current.BuildString); // 42
            Console.WriteLine($"Local: versionName={localVersionName}, versionCode={localVersionCode}");

            // ── 2. Fetch remote manifest ──────────────────────────────────
            var manifestUrl =
                $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/version.json";

            var manifestJson = await _httpClient.GetStringAsync(manifestUrl);
            var manifest = JsonSerializer.Deserialize<VersionManifest>(manifestJson, _jsonOptions);

            if (manifest is null || string.IsNullOrWhiteSpace(manifest.VersionName))
            {
                Console.WriteLine("Remote version.json is empty or malformed.");
                return null;
            }

            Console.WriteLine($"Remote: versionName={manifest.VersionName}, versionCode={manifest.VersionCode}");

            // ── 3. Decide whether an update is possible ───────────────────
            // Primary signal: Android WILL NOT install unless versionCode increases.
            var codeIsNewer = manifest.VersionCode > localVersionCode;

            // Secondary signal: only used if versionCode is missing from the manifest
            // (older releases published before this change). Display-only otherwise.
            var nameIsNewer = VersionHelper.IsGreater(manifest.VersionName, localVersionName);

            var updateAvailable = manifest.VersionCode > 0
                ? codeIsNewer                          // strict — guarantees install succeeds
                : nameIsNewer;                         // fallback for legacy releases

            if (!updateAvailable)
            {
                Console.WriteLine("No update available.");
                return null;
            }

            // ── 4. Build the APK download URL ─────────────────────────────
            var apkFileName = !string.IsNullOrWhiteSpace(manifest.ApkFileName)
                ? manifest.ApkFileName
                : GuessApkFileName(manifest.VersionName);

            var downloadUrl =
                $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/{apkFileName}";

            return new UpdateInfo
            {
                Version = manifest.VersionName,
                VersionCode = manifest.VersionCode,
                DownloadUrl = downloadUrl,
                ReleaseNotes = manifest.ReleaseNotes
            };
        }
        catch (Exception ex)
        {
            // Network errors, 404s (no release yet), JSON errors, etc.
            Console.WriteLine($"Update check failed: {ex.Message}");
            return null;
        }
    }

    private static int ParseIntSafe(string? s) =>
        int.TryParse(s, out var v) ? v : 0;

    private static string GuessApkFileName(string version) =>
        $"SampleMauiApp-{version}.apk"; // last-resort fallback only
}