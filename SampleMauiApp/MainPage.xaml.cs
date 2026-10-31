
using SampleMauiApp.Services;

namespace SampleMauiApp
{
    public partial class MainPage : ContentPage
    {
        int count = 0;

        public MainPage()
        {
            InitializeComponent();
        }

        private void OnCounterClicked(object? sender, EventArgs e)
        {
            count += 2;

            if (count == 1)
                CounterBtn.Text = $"Clicked {count} time";
            else
                CounterBtn.Text = $"Clicked {count} times";

            SemanticScreenReader.Announce(CounterBtn.Text);
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await CheckForUpdates();

            async Task CheckForUpdates()
            {
                var updateService = new UpdateService();
                // **CHANGE THESE** to your GitHub username and repository name
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
}
