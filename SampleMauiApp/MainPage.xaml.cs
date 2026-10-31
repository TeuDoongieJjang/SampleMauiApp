using SampleMauiApp.Services;

namespace SampleMauiApp
{
    public partial class MainPage : ContentPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();

            var updateService = new UpdateService();
            var updateInfo = await updateService.CheckForUpdateAsync("TeuDoongieJjang", "SampleMauiApp");

            if (updateInfo != null)
            {
                bool answer = await DisplayAlertAsync(
                    "Update Available",
                    $"Version {updateInfo.Version} (build {updateInfo.VersionCode}) is available " +
                    $"– you have build {AppInfo.Current.BuildString}.\n\n" +
                    "Download and install now?",
                    "Yes", "No");

                if (answer)
                {
                    var installer = new ApkInstaller();
                    await installer.InstallApkAsync(updateInfo.DownloadUrl);
                }
            }
        }
    }
}