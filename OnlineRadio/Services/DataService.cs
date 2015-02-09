using GalaSoft.MvvmLight;
using Newtonsoft.Json;
using OnlineRadio.Model;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Diagnostics;
using OnlineRadio.Helper;

namespace OnlineRadio.Services
{
    public class DataService : ViewModelBase
    {
        public DataService()
        {

        }

        private void InitializeLiveTile()
        {
        }

        public void InitialLoad()
        {
            LoadSettings();
            LoadFavStations();
            LoadRecentStations();
            LoadStationCategories();
        }

        private List<ItemEx> _channels;
        private List<ItemEx> _recentlyPlayedChannels;
        private List<ItemEx> _favChannels;
        private List<string> _stationCategories;
        public List<ItemEx> LoadChannels()
        {
            _channels = SQLiteWrapper.Instance.GetAllItems();
            //await Task.Factory.StartNew(async () => { await DownloadFromCloud(); });
            return _channels;
        }

        private async Task DownloadFromCloud()
        {
            var newChannels = new List<Item>();
            string remoteConfigFileUrl = "https://thehinduadrotator.blob.core.windows.net/adrotator/Channels.json";

            HttpClientHandler handler = new HttpClientHandler();
            handler.AutomaticDecompression = System.Net.DecompressionMethods.Deflate | System.Net.DecompressionMethods.GZip;
            HttpClient client = new HttpClient(handler);
            string remotConfiguration = await client.GetStringAsync(remoteConfigFileUrl);
            if (!string.IsNullOrEmpty(remotConfiguration))
            {
                var remoteRootObject = await Task.Factory.StartNew(() => JsonConvert.DeserializeObject<RootObject>(remotConfiguration));
                //var remoteRootObject = JsonConvert.DeserializeObject<RootObject>(remotConfiguration);
                if (remoteRootObject != null && remoteRootObject.rss != null && remoteRootObject.rss.channel != null)
                {
                    newChannels = remoteRootObject.rss.channel.item;
                }
            }

            bool isTableCreated = await SQLiteWrapper.Instance.CreateTableIfNot();
            if (isTableCreated == true)
            {
                foreach (var channel in newChannels)
                {
                    SQLiteWrapper.Instance.UpdateItemInTable(channel);
                }
                _channels = SQLiteWrapper.Instance.GetAllItems();
            }
            else
            {
                foreach (var channel in newChannels)
                {
                    if (_channels.Find(o => o.chlink == channel.chlink) == null)
                    {
                        ItemEx item = new ItemEx();
                        item.category = channel.category;
                        item.chcat = channel.chcat;
                        item.chdescription = channel.chdescription;
                        item.chlink = channel.chlink;
                        item.chtype = channel.chtype;
                        item.title = channel.title;
                        _channels.Add(item);
                    }
                }
            }
        }
        public List<string> LoadStationCategories()
        {
            if (_stationCategories != null)
            {
                return _stationCategories;
            }
            if (_channels != null)
            {
                if (_stationCategories == null)
                {
                    _stationCategories = new List<string>();
                }
                foreach (var item in _channels)
                {
                    if (!_stationCategories.Contains(item.category))
                    {
                        _stationCategories.Add(item.category);
                    }
                }
            }
            return _stationCategories;
        }
        
        public List<ItemEx> LoadFavStations()
        {
            if (_channels != null)
            {
                if (_favChannels == null)
                {
                    _favChannels = new List<ItemEx>();
                }
                _favChannels = SQLiteWrapper.Instance.GetFavChannels();
            }
            return _favChannels;
        }

        private void LoadSettings()
        {
            throw new NotImplementedException();
        }

        public async Task<List<ItemEx>> LoadRecentStations()
        {            
            if (AppHelper.Instance.GetRecentChannels() != null)
            {
                _recentlyPlayedChannels = await AppHelper.Instance.GetRecentChannels();
            }
            if (_recentlyPlayedChannels == null)
            {
                _recentlyPlayedChannels = new List<ItemEx>();
            }
            //_recentlyPlayedChannels.Reverse();
            return _recentlyPlayedChannels;
        }

        public List<ItemEx> AddToRecentlyPlayedList(ItemEx item)
        {
            if (_recentlyPlayedChannels == null)
            {
                _recentlyPlayedChannels = new List<ItemEx>();
            }
            else
            {
                _recentlyPlayedChannels.Reverse();
                if (_recentlyPlayedChannels.FirstOrDefault(o => o.chlink == item.chlink) == null)
                {
                    if (_recentlyPlayedChannels.Count > 9)
                    {
                        _recentlyPlayedChannels.RemoveAt(0);
                    }
                    _recentlyPlayedChannels.Add(item);
                }
                else
                {
                    if (_recentlyPlayedChannels.Count > 9)
                    {
                        _recentlyPlayedChannels.RemoveAt(0);
                    }
                    _recentlyPlayedChannels.Remove(item);
                    _recentlyPlayedChannels.Add(item);
                }
            }
            AppHelper.Instance.SetRecentChannels(_recentlyPlayedChannels);
            _recentlyPlayedChannels.Reverse();
            return _recentlyPlayedChannels;
        }
        
        public List<ItemEx> RemoveFromFavChannels(ItemEx item)
        {
            if (_favChannels != null && _favChannels.Contains(item))
            {
                _favChannels.Remove(item);
            }
            SQLiteWrapper.Instance.UpdateItemExInTable(item);
            return _favChannels;
        }

        public List<ItemEx> AddToFavChannels(ItemEx item)
        {
            if (_favChannels != null && !_favChannels.Contains(item))
            {
                _favChannels.Add(item);
            }
            else if (_favChannels == null)
            {
                _favChannels = new List<ItemEx>();
                _favChannels.Add(item);
            }
            SQLiteWrapper.Instance.UpdateItemExInTable(item);
            return _favChannels;
        }
    }
}
