using GalaSoft.MvvmLight;
using GalaSoft.MvvmLight.Command;
using GalaSoft.MvvmLight.Threading;
using GalaSoft.MvvmLight.Views;
using Newtonsoft.Json;
using OnlineRadio.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Windows.Media;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace OnlineRadio.ViewModel
{
    public class MainViewModel : ViewModelBase
    {
        public MainViewModel(DataService dataService)
        {
            _navigationService = new NavigationService();
            _navigationService.Configure("Category", typeof(CategoryPage));
            _navigationService.Configure("MainPage", typeof(MainPage));
            _dataService = dataService;
            Intialize();
            LoadChannelsCommand = new RelayCommand(LoadChannels);
            CategoryItemSelectedCommand = new RelayCommand<ItemClickEventArgs>(CategoryItemClicked);
            CategoryChannelSelectedCommand = new RelayCommand<ItemClickEventArgs>(CategoryChannelSelected);
            ChannelSelectedCommand = new RelayCommand<ItemClickEventArgs>(ChannelSelected);
            AddToFavStationsCommand = new RelayCommand(AddToFavStations);
            RemoveFromFavStationsCommand = new RelayCommand(RemoveFromFavStations);            
        }

        private static bool isLoaded = false;
        private void RemoveFromFavStations()
        {
            if (SelectedChannel != null)
            {
                SelectedChannel.IsFav = false;
                FavChannels.Clear();
                var items = _dataService.RemoveFromFavChannels(SelectedChannel);
                foreach(var item in items)
                {
                    FavChannels.Add(item);
                }
                if(FavChannels.Count > 0)
                {
                    AreFavoritesPresent = true;
                }
                else
                {
                    AreFavoritesPresent = false;
                }
            }
        }

        private void AddToFavStations()
        {
            if(SelectedChannel != null)
            {
                SelectedChannel.IsFav = true;
                FavChannels.Clear();
                var items = _dataService.AddToFavChannels(SelectedChannel);
                foreach (var item in items)
                {
                    FavChannels.Add(item);
                }
                if(FavChannels.Count > 0)
                {
                    AreFavoritesPresent = true;
                }
                else
                {
                    AreFavoritesPresent = false;
                }
            }
        }

        private void Intialize()
        {
            //if (_selectedChannel == null)
            //    _selectedChannel = new Item();
            if(_recentlyPlayedChannels == null)
            {
                _recentlyPlayedChannels = new ObservableCollection<ItemEx>();
            }
            if (_favChannels == null)
                _favChannels = new ObservableCollection<ItemEx>();
        }

        #region RelayCommands
        public RelayCommand LoadChannelsCommand { get; private set; }
        public RelayCommand<ItemClickEventArgs> CategoryItemSelectedCommand { get; private set; }
        public RelayCommand<ItemClickEventArgs> CategoryChannelSelectedCommand { get; private set; }
        public RelayCommand<ItemClickEventArgs> ChannelSelectedCommand { get; private set; }
        public RelayCommand AddToFavStationsCommand { get; private set; }
        public RelayCommand RemoveFromFavStationsCommand { get; private set; }
        public RelayCommand AddToFavCommand { get; private set; }
        public RelayCommand RemoveFromFavCommand { get; private set; }
        #endregion

        #region Properities

        public List<ItemEx> Channels
        {
            get
            {
                return _channels;
            }
            set
            {
                _channels = value;
                RaisePropertyChanged("Channels");
            }
        }

        public ObservableCollection<ItemEx> RecentlyPlayedChannels
        {
            get
            {
                return _recentlyPlayedChannels;
            }
            set
            {
                _recentlyPlayedChannels = value;
                RaisePropertyChanged("RecentlyPlayedChannels");
            }
        }

        public ObservableCollection<ItemEx> FavChannels
        {
            get
            {
                return _favChannels;
            }
            set
            {
                _favChannels = value;
                RaisePropertyChanged("FavChannels");
            }
        }

        public List<string> ListOfStationCategories
        {
            get
            {
                return _listOfStationCategories;
            }
            set
            {
                _listOfStationCategories = value;
                RaisePropertyChanged("ListOfStationCategories");
            }
        }

        public string SelectedCategory
        {
            get
            {
                return _selectedCategory;
            }
            set
            {
                _selectedCategory = value;
                RaisePropertyChanged("SelectedCategory");
            }
        }

        private ItemEx _selectedChannel;
        public ItemEx SelectedChannel
        {
            get
            {
                return _selectedChannel;
            }
            set
            {
                _selectedChannel = value;
                RaisePropertyChanged("SelectedChannel");
            }
        }

        public List<ItemEx> ListOfSelectedCategoryStations
        {
            get
            {
                return _listOfSelectedCategoryStations;
            }

            set
            {
                _listOfSelectedCategoryStations = value;
                RaisePropertyChanged("ListOfSelectedCategoryStations");
            }
        }

        public bool AreFavoritesPresent
        {
            get
            {
                return _areFavoritesPresent;
            }
            set
            {
                _areFavoritesPresent = value;
                RaisePropertyChanged("AreFavoritesPresent");
            }
        }

        public bool AreRecentChannelsPresent
        {
            get
            {
                return _areRecentChannelsPresent;
            }
            set
            {
                _areRecentChannelsPresent = value;
                RaisePropertyChanged("AreRecentChannelsPresent");
            }
        }

        public bool IsDataLoading
        {
            get
            {
                return _isDataLoading;
            }
            set
            {
                _isDataLoading = value;
                RaisePropertyChanged("IsDataLoading");
            }
        }


        #endregion

        #region Private fields
        private List<ItemEx> _channels = null;
        private ObservableCollection<ItemEx> _recentlyPlayedChannels = null;
        private ObservableCollection<ItemEx> _favChannels = null;
        private List<ItemEx> _listOfSelectedCategoryStations = null;
        private List<string> _listOfStationCategories = null;
        private string _selectedCategory = null;
        private DataService _dataService = null;
        private NavigationService _navigationService = null;
        private bool _areFavoritesPresent;
        private bool _areRecentChannelsPresent;
        private bool _isDataLoading = true;
        #endregion

        private async void LoadChannels()
        {
            IsDataLoading = true;
            await Task.Delay(500);
            if (isLoaded == false)
            {
                await Task.Factory.StartNew(async () =>
                {
                    var channels = _dataService.LoadChannels();
                    await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    {
                        Channels = channels;
                    });

                    await LoadStationCategories();
                    await LoadRecentStations();
                    await LoadFavStations();
                });
                isLoaded = true;
            }
            IsDataLoading = false;
        }

        private async Task LoadStationCategories()
        {
            await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                ListOfStationCategories = _dataService.LoadStationCategories();
            });            
        }

        private async Task LoadRecentStations()
        {
            await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, async() =>
            {
                var items = await _dataService.LoadRecentStations();
                if(items != null)
                {
                    foreach(var item in items)
                    {
                        RecentlyPlayedChannels.Add(item);
                    }                    
                }
                
                if(RecentlyPlayedChannels.Count > 0)
                {
                    AreRecentChannelsPresent = true;
                }
                else
                {
                    AreRecentChannelsPresent = false;
                }
            });     
        }

        private async Task LoadFavStations()
        {
            await DispatcherHelper.UIDispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                FavChannels.Clear();
                var items = _dataService.LoadFavStations();
                foreach(var item in items)
                {
                    FavChannels.Add(item);
                }
                if (FavChannels.Count > 0)
                    AreFavoritesPresent = true;
                else
                {
                    AreFavoritesPresent = false;
                }
            });  
        }

        private void CategoryItemClicked(ItemClickEventArgs e)
        {
            SelectedCategory = e.ClickedItem.ToString();
            _navigationService.NavigateTo("Category");
            if (Channels != null)
            {
                ListOfSelectedCategoryStations = Channels.FindAll(o => o.category == SelectedCategory);
            }
        }

        private void CategoryChannelSelected(ItemClickEventArgs obj)
        {
            SelectedChannel = (ItemEx)obj.ClickedItem;
            if (SelectedChannel != null)
            {
                PlayAndAddToRecent(SelectedChannel);
            }
            _navigationService.GoBack();
        }

        private void PlayAndAddToRecent(ItemEx item)
        {
            var items = _dataService.AddToRecentlyPlayedList(item);
            RecentlyPlayedChannels.Clear();
            foreach(var tempItem in items )
            {
               if(RecentlyPlayedChannels.FirstOrDefault(o=> o.chlink == tempItem.chlink) == null)
               {
                   RecentlyPlayedChannels.Add(tempItem);
               }
            }
            if(RecentlyPlayedChannels.Count > 0)
            {
                AreRecentChannelsPresent = true;
            }
            else
            {
                AreRecentChannelsPresent = false;
            }
                        
            DependencyObject rootGrid = VisualTreeHelper.GetChild(Window.Current.Content, 0);
            if (rootGrid != null)
            {
                var mediaPlayer = (MediaElement)VisualTreeHelper.GetChild(rootGrid, 0);
                if (mediaPlayer != null)
                {
                    mediaPlayer.Source = new Uri(SelectedChannel.chlink, UriKind.RelativeOrAbsolute);
                    mediaPlayer.Play();
                }
            }
        }

        private void ChannelSelected(ItemClickEventArgs obj)
        {
            SelectedChannel = (ItemEx)obj.ClickedItem;
            if (SelectedChannel != null)
            {
                PlayAndAddToRecent(SelectedChannel);
            }
        }
    }
}