using Android.Content;

namespace SampleMauiApp.Services // <-- CRITICAL: Must match the shared file
{
    public partial class ApkInstaller
    {
        public partial async Task InstallApkAsync(string apkUrl)
        {
            // 1. Download the APK
            var httpClient = new HttpClient();
            var apkBytes = await httpClient.GetByteArrayAsync(apkUrl);
            var apkPath = Path.Combine(FileSystem.AppDataDirectory, "update.apk");
            await File.WriteAllBytesAsync(apkPath, apkBytes);

            // 2. Create the URI using FileProvider
            var file = new Java.IO.File(apkPath);
            var apkUri = AndroidX.Core.Content.FileProvider.GetUriForFile(
                Platform.AppContext, // <-- FIXED: Uses MAUI's AppContext
                "com.companyname.samplemauiapp.fileprovider", // Ensure this matches your Manifest!
                file);

            // 3. Create the intent to install the APK
            var intent = new Intent(Intent.ActionView);
            intent.SetDataAndType(apkUri, "application/vnd.android.package-archive");
            intent.AddFlags(ActivityFlags.NewTask);
            intent.AddFlags(ActivityFlags.GrantReadUriPermission);

            // 4. Launch the installer
            Platform.AppContext.StartActivity(intent); // <-- FIXED
        }
    }
}