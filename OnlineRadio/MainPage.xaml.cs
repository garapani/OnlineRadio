using Newtonsoft.Json;
using OnlineRadio.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Windows.Media;
using Windows.Networking.Connectivity;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace OnlineRadio
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            this.InitializeComponent();
            this.Loaded += MainPage_Loaded;
            this.Unloaded += MainPage_Unloaded;
                        
            //if (stationCategories != null && stationCategories.SelectedItems != null && stationCategories.SelectedItems.Count > 0)
            //{
            //    stationCategories.SelectedItems.Clear();
            //}
        }

        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged += Window_SizeChanged;
            DetermineVisualState();
        }

        private void Window_SizeChanged(object sender, Windows.UI.Core.WindowSizeChangedEventArgs e)
        {
            DetermineVisualState();
        }

        void MainPage_Unloaded(object sender, RoutedEventArgs e)
        {
            Window.Current.SizeChanged -= Window_SizeChanged;           
        }

        MediaElement mediaPlayer = null;
        private SystemMediaTransportControls _systemMediaControls = null;
        ApplicationDataContainer _roamingData = ApplicationData.Current.RoamingSettings;
        ApplicationDataContainer _localData = ApplicationData.Current.LocalSettings;

        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            //stationCategories.SelectedIndex = -1;
            base.OnNavigatedTo(e);
        }

        public static bool CheckInternetConnectivity()
        {
            var internetProfile = NetworkInformation.GetInternetConnectionProfile();
            if (internetProfile == null)
                return false;

            return (internetProfile.GetNetworkConnectivityLevel() == NetworkConnectivityLevel.InternetAccess);
        }

        private bool ReadAppRatingSetting()
        {
            if (_localData.Values["AppRatingDone"] != null)
                return (bool)_localData.Values["AppRatingDone"];

            if (_roamingData.Values["AppRatingDone"] != null)
                return (bool)_roamingData.Values["AppRatingDone"];

            return false;
        }

        private void SaveAppRatingSetting()
        {
            // for this setting, it's either saved or not, so we save true as the default value
            _localData.Values["AppRatingDone"] = true;
            _roamingData.Values["AppRatingDone"] = true;

        }

        private async Task AskForAppRating()
        {
            if (!CheckInternetConnectivity())
                return;

            //if (ReadAppRatingSetting())
            //    return;

            MessageDialog msg;
            bool retry = false;
            msg = new MessageDialog("Kindly, help us provide a better service, would you like to review & rate \"Online Radio\"?");
            msg.Commands.Add(new UICommand("Rate", new UICommandInvokedHandler(this.OpenStoreRating)));
            msg.Commands.Add(new UICommand("Not now", (uiCommand) => { }));
            msg.Commands.Add(new UICommand("No, thanks", new UICommandInvokedHandler(this.IgnoreRating)));
            msg.DefaultCommandIndex = 0;
            msg.CancelCommandIndex = 1;
            await msg.ShowAsync();

            while (retry)
            {
                try { await msg.ShowAsync(); }
                catch (Exception ex) { }
            }
        }
       
        private void IgnoreRating(IUICommand command)
        {
            SaveAppRatingSetting();
        }

        // Launches the store app, using the application storeID
        private async void OpenStoreRating(IUICommand command)
        {
            await Windows.System.Launcher.LaunchUriAsync(new Uri("ms-windows-store:REVIEW?PFN=2e5457dc-ef9c-44a2-bc8a-3652d0752e9d"));

            SaveAppRatingSetting();

        }
                         
        private void likeButton_Tapped(object sender, Windows.UI.Xaml.Input.TappedRoutedEventArgs e)
        {
            AskForAppRating();
        }

        private void AdControl_AdRefreshed(object sender, RoutedEventArgs e)
        {

        }

        private void DetermineVisualState()
        {
            string visualState = "FullScreenLandscape";
            var windowWidth = Window.Current.Bounds.Width;
            var windowHeight = Window.Current.Bounds.Height;

            if (windowWidth <= 500)
            {
                visualState = "Snapped"; //+"_Detail";
            }
            else if (windowWidth <= 1366)
            {
                if (windowWidth < windowHeight)
                {
                    visualState = "FullScreenPortrait";// +"_Detail";
                }
                else
                {
                    visualState = "Filled";
                }
            }

            VisualStateManager.GoToState(this, visualState, true);
        }
    }
}
