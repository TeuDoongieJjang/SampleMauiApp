namespace SampleMauiApp.Services
{
    public partial class ApkInstaller
    {
#if ANDROID
        public partial Task InstallApkAsync(string apkUrl);
#else
        public Task InstallApkAsync(string apkUrl) =>
            throw new PlatformNotSupportedException("APK installation is supported only on Android.");
#endif
    }
}
