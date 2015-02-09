using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace OnlineRadio
{
    public sealed partial class NowPlayingUserControl : UserControl
    {
        MediaElement mediaPlayer = null;
        private SystemMediaTransportControls _systemMediaControls = null;

        public NowPlayingUserControl()
        {
            this.InitializeComponent();
            this.Loaded += NowPlayingUserControl_Loaded;
            this.Unloaded += NowPlayingUserControl_Unloaded;
            InitializeTransportControls();
        }

        void NowPlayingUserControl_Unloaded(object sender, RoutedEventArgs e)
        {
            if (mediaPlayer != null)
            {
                mediaPlayer.CurrentStateChanged -= mediaPlayer_CurrentStateChanged;
                mediaPlayer.MarkerReached -= mediaPlayer_MarkerReached;
            }
        }

        void NowPlayingUserControl_Loaded(object sender, RoutedEventArgs e)
        {
            DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
            mediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
            if (mediaPlayer != null)
            {
                mediaPlayer.AudioCategory = AudioCategory.BackgroundCapableMedia;
                mediaPlayer.AudioDeviceType = AudioDeviceType.Multimedia;
                mediaPlayer.AreTransportControlsEnabled = true;
                mediaPlayer.CurrentStateChanged += mediaPlayer_CurrentStateChanged;
                mediaPlayer.MarkerReached += mediaPlayer_MarkerReached;
            }
        }

        void InitializeTransportControls()
        {
            _systemMediaControls = SystemMediaTransportControls.GetForCurrentView();
            if (_systemMediaControls != null)
            {
                _systemMediaControls.ButtonPressed += systemControls_ButtonPressed;
                _systemMediaControls.IsPlayEnabled = true;
                _systemMediaControls.IsPauseEnabled = true;
            }
        }

        void mediaPlayer_CurrentStateChanged(object sender, RoutedEventArgs e)
        {
            bufferProgress.Text = "";
            progressBar.Visibility = Visibility.Collapsed;
            if (mediaPlayer != null)
            {
                if (mediaPlayer.CurrentState == MediaElementState.Playing)
                {
                    pauseButtonImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    playButtonImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    bufferProgress.Text = "playing...";
                }
                else if (mediaPlayer.CurrentState == MediaElementState.Paused)
                {
                    playButtonImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    pauseButtonImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
                    bufferProgress.Text = "paused";
                }
                else if (mediaPlayer.CurrentState == MediaElementState.Buffering)
                {
                    progressBar.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
                    int percentage = (int)(mediaPlayer.BufferingProgress * 100);
                    bufferProgress.Text = "buffering... " + percentage.ToString() + " %";
                }
                else if (mediaPlayer.CurrentState == MediaElementState.Opening)
                {
                    bufferProgress.Text = "opening...";
                    progressBar.Visibility = Windows.UI.Xaml.Visibility.Visible;
                }
                else if(mediaPlayer.CurrentState == MediaElementState.Stopped)
                {
                    bufferProgress.Text = "stopped.";                    
                }
                else if(mediaPlayer.CurrentState == MediaElementState.Closed)
                {
                    bufferProgress.Text = "Internet connectivity issue.";
                }
            }
            SyncPlaybackStatusToMediaElementState();
            //UpdateSongInfoManually();
        }

        private void SyncPlaybackStatusToMediaElementState()
        {
            if (_systemMediaControls != null && mediaPlayer != null)
            {
                switch (mediaPlayer.CurrentState)
                {
                    case MediaElementState.Closed:
                        _systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Closed;
                        break;

                    case MediaElementState.Opening:
                        break;

                    case MediaElementState.Buffering:
                        break;

                    case MediaElementState.Paused:
                        _systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Paused;
                        break;

                    case MediaElementState.Playing:
                        _systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Playing;
                        break;

                    case MediaElementState.Stopped:
                        _systemMediaControls.PlaybackStatus = MediaPlaybackStatus.Stopped;
                        break;
                }
            }
        }

        void systemControls_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Pause:
                    PauseMedia();
                    break;
                case SystemMediaTransportControlsButton.Play:
                    PlayMedia();
                    break;
                default:
                    break;
            }
        }
        
        private async void PauseMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.Pause();
                }
            });
        }

        private async void PlayMedia()
        {
            await Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, () =>
            {
                if (mediaPlayer != null)
                {
                    mediaPlayer.Play();
                }
            });
        }

        void mediaPlayer_MarkerReached(object sender, TimelineMarkerRoutedEventArgs e)
        {
            Dictionary<string, string> songAttribs = new Dictionary<string, string>();
            string playerFeed = System.Net.WebUtility.UrlDecode(e.Marker.Text);
            char[] delims = { '&' };
            string[] Attribs = playerFeed.Split(delims);

            foreach (String attrib in Attribs)
            {
                string[] keypair = attrib.Split('=');
                string key = "";
                string value = "";

                try
                {
                    key = keypair[0];
                }
                catch
                {
                    key = null;
                }

                if (key != null)
                {
                    try
                    {
                        value = keypair[1];
                    }
                    catch
                    {
                        value = "";
                    }

                    songAttribs.Add(keypair[0], keypair[1]);
                }
            }
            var updater = _systemMediaControls.DisplayUpdater;
            updater.Type = MediaPlaybackType.Music;
            updater.MusicProperties.Title = songAttribs["title"];
            updater.MusicProperties.Artist = songAttribs["artist"];
            //updater.MusicProperties.Duration = 0;
            songDetails.Text = "Artist: " + updater.MusicProperties.Artist + "Title: " + updater.MusicProperties.Title + " playerFeed: " + playerFeed;
        }


        private void pauseButton_Click(object sender, RoutedEventArgs e)
        {
            pauseButtonImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            playButtonImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (mediaPlayer != null)
                mediaPlayer.Pause();
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            playButtonImage.Visibility = Windows.UI.Xaml.Visibility.Collapsed;
            pauseButtonImage.Visibility = Windows.UI.Xaml.Visibility.Visible;
            if (mediaPlayer != null)
                mediaPlayer.Play();
        }
    }
}
