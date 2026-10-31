using SampleMauiApp.Models;
using System.Text.Json;

namespace SampleMauiApp.Services;

public class UpdateService
{
    private static readonly HttpClient _httpClient = new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    private static readonly JsonSerializerOptions _jsonOptions = new() { PropertyNameCaseInsensitive = true };

    public async Task<UpdateInfo?> CheckForUpdateAsync(string repoOwner, string repoName)
    {
        try
        {
            var localVersionName = AppInfo.Current.VersionString;
            var localVersionCode = int.TryParse(AppInfo.Current.BuildString, out var code) ? code : 0;

            // 1. Fetch remote manifest
            var manifestUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/version.json";
            var manifestJson = await _httpClient.GetStringAsync(manifestUrl);

            var manifest = JsonSerializer.Deserialize<VersionManifest>(manifestJson, _jsonOptions);

            if (manifest is null || string.IsNullOrWhiteSpace(manifest.VersionName))
            {
                throw new Exception("version.json is empty or malformed.");
            }

            // 2. Check versions
            var codeIsNewer = manifest.VersionCode > localVersionCode;
            var nameIsNewer = VersionHelper.IsGreater(manifest.VersionName, localVersionName);

            var updateAvailable = manifest.VersionCode > 0 ? codeIsNewer : nameIsNewer;

            if (!updateAvailable)
            {
                // Show an alert so we know the check succeeded, but no update is needed
                await MainThread.InvokeOnMainThreadAsync(() =>
                    Application.Current!.MainPage!.DisplayAlertAsync("Debug: No Update",
                        $"Check succeeded.\nLocal Code: {localVersionCode}\nRemote Code: {manifest.VersionCode}\n\nNo update needed.", "OK"));
                return null;
            }

            // 3. Build Download URL
            var apkFileName = !string.IsNullOrWhiteSpace(manifest.ApkFileName)
                ? manifest.ApkFileName
                : $"SampleMauiApp-{manifest.VersionName}.apk";

            var downloadUrl = $"https://github.com/{repoOwner}/{repoName}/releases/latest/download/{apkFileName}";

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
            // SHOW THE ERROR ON SCREEN
            await MainThread.InvokeOnMainThreadAsync(() =>
                Application.Current!.MainPage!.DisplayAlertAsync("Debug: Update Failed", ex.Message, "OK"));
            return null;
        }
    }
}