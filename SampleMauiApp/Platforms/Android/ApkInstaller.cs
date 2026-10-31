using Android.Content;
using Android.Provider;

namespace SampleMauiApp.Services
{
    public partial class ApkInstaller
    {
        public partial async Task InstallApkAsync(string apkUrl)
        {
            try
            {
                // 1. Download to AppDataDirectory (persistent, shareable via <files-path>)
                var apkFileName = "update.apk";
                var apkPath = Path.Combine(FileSystem.AppDataDirectory, apkFileName);

                using var httpClient = new HttpClient();
                await using (var remoteStream = await httpClient.GetStreamAsync(apkUrl))
                await using (var localStream = File.Create(apkPath))
                {
                    await remoteStream.CopyToAsync(localStream);
                }

                // 2. Create content:// URI via FileProvider
                var file = new Java.IO.File(apkPath);
                var authority = $"{Platform.AppContext.PackageName}.fileprovider";
                var apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                    Platform.AppContext, authority, file);

                // 3. On Android 8.0+ verify that this app is allowed to install packages
                if (OperatingSystem.IsAndroidVersionAtLeast(26))
                {
                    var packageManager = Platform.AppContext.PackageManager;
                    if (!packageManager.CanRequestPackageInstalls())
                    {
                        // Redirect the user to the "Install unknown apps" settings page
                        var settingsIntent = new Intent(Settings.ActionManageUnknownAppSources);
                        settingsIntent.SetData(
                            Android.Net.Uri.Parse($"package:{Platform.AppContext.PackageName}"));
                        settingsIntent.AddFlags(ActivityFlags.NewTask);
                        Platform.AppContext.StartActivity(settingsIntent);

                        // Inform the caller that the user must enable the permission manually
                        throw new InvalidOperationException(
                            "The user must enable 'Install unknown apps' for this app before the update can proceed.");
                    }
                }

                // 4. Launch the system installer
                var installIntent = new Intent(Intent.ActionView);
                installIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
                installIntent.AddFlags(ActivityFlags.NewTask);
                installIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

                Platform.AppContext.StartActivity(installIntent);
            }
            catch (Exception ex)
            {
                // Log or rethrow as appropriate for your app
                Console.WriteLine($"APK installation failed: {ex}");
                throw;
            }
        }
    }
}