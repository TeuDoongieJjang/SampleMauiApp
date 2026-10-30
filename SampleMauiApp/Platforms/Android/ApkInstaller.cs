using Android.Content;
using Android.Provider;

namespace SampleMauiApp.Services
{
    public partial class ApkInstaller
    {
        public partial async Task InstallApkAsync(string apkUrl)
        {
            // 1. Download the APK to the cache directory
            var httpClient = new HttpClient();
            var apkBytes = await httpClient.GetByteArrayAsync(apkUrl);
            var apkPath = Path.Combine(FileSystem.CacheDirectory, "update.apk");
            await File.WriteAllBytesAsync(apkPath, apkBytes);

            // 2. Create the URI using FileProvider
            var file = new Java.IO.File(apkPath);
            var authority = $"{Platform.AppContext.PackageName}.fileprovider";
            var apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                Platform.AppContext,
                authority,
                file);

            // 3. Check for INSTALL_PACKAGES permission (Android 8.0+)
            if (OperatingSystem.IsAndroidVersionAtLeast(26))
            {
                var packageManager = Platform.AppContext.PackageManager;
                if (!packageManager.CanRequestPackageInstalls())
                {
                    // Guide the user to enable "Install unknown apps" for this app
                    var intent = new Intent(Settings.ActionManageUnknownAppSources);
                    intent.SetData(Android.Net.Uri.Parse($"package:{Platform.AppContext.PackageName}"));
                    intent.AddFlags(ActivityFlags.NewTask);
                    Platform.AppContext.StartActivity(intent);
                    return; // Exit; the user must manually enable the permission
                }
            }

            // 4. Create and launch the install intent
            var installIntent = new Intent(Intent.ActionView);
            installIntent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
            installIntent.AddFlags(ActivityFlags.NewTask);
            installIntent.AddFlags(ActivityFlags.GrantReadUriPermission);

            Platform.AppContext.StartActivity(installIntent);
        }
    }
}