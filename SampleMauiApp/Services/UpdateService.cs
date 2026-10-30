using System.Text.Json;

namespace SampleMauiApp.Services
{
    public class UpdateService
    {
        private static readonly HttpClient _httpClient = new HttpClient();

        public async Task<UpdateInfo?> CheckForUpdateAsync(string repoOwner, string repoName)
        {
            if (!_httpClient.DefaultRequestHeaders.Contains("User-Agent"))
                _httpClient.DefaultRequestHeaders.Add("User-Agent", "MAUI-App");

            var url = $"https://api.github.com/repos/{repoOwner}/{repoName}/releases/latest";

            try
            {
                var response = await _httpClient.GetAsync(url);
                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadAsStringAsync();
                var release = JsonSerializer.Deserialize<GitHubRelease>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (release == null) return null;

                var latestVersion = release.TagName.TrimStart('v');
                var currentVersion = AppInfo.Current.VersionString; // e.g., "1.0.0"

                if (Version.Parse(latestVersion) > Version.Parse(currentVersion))
                {
                    var apkAsset = release.Assets.FirstOrDefault(a => a.Name.EndsWith(".apk"));
                    if (apkAsset != null)
                    {
                        return new UpdateInfo
                        {
                            Version = latestVersion,
                            DownloadUrl = apkAsset.BrowserDownloadUrl,
                            ReleaseNotes = release.Body
                        };
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Update check failed: {ex.Message}");
            }
            return null;
        }
    }

    // DTOs
    public class GitHubRelease { public string TagName { get; set; } public string Body { get; set; } public List<GitHubAsset> Assets { get; set; } }
    public class GitHubAsset { public string Name { get; set; } public string BrowserDownloadUrl { get; set; } }
    public class UpdateInfo { public string Version { get; set; } public string DownloadUrl { get; set; } public string ReleaseNotes { get; set; } }
}
