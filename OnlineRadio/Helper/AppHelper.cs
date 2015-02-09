using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Serialization;
using Windows.Storage;

namespace OnlineRadio.Helper
{
    public class AppHelper
    {
        private string _recentSettings = "RecentSettings";
        public async Task<List<ItemEx>> GetRecentChannels()
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var fileToGet = await folder.CreateFileAsync("xmlFile.txt", CreationCollisionOption.OpenIfExists);
            string text = await FileIO.ReadTextAsync(fileToGet);
            if (string.IsNullOrEmpty(text))
                return null;
            else
            {
                return await DeserializeJsonToList(text);
                //return DeserializeXmlToList(text);
            }
        }

        public async void SetRecentChannels(List<ItemEx> items)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            var fileToGet = await folder.CreateFileAsync("xmlFile.txt", CreationCollisionOption.OpenIfExists);
            await FileIO.WriteTextAsync(fileToGet, await SerializeListToText(items));
        }

        public Task<List<ItemEx>> DeserializeJsonToList(string text)
        {
            return JsonConvert.DeserializeObjectAsync <List<ItemEx>>(text);
        }

        public Task<string> SerializeListToText(List<ItemEx> listOfItems)
        {
            return JsonConvert.SerializeObjectAsync(listOfItems);
        }

       

        private string SerializeListToXml(List<ItemEx> List)
        {
            try
            {
                XmlSerializer xmlIzer = new XmlSerializer(typeof(List<ItemEx>));
                var writer = new StringWriter();
                xmlIzer.Serialize(writer, List);
                return writer.ToString();
            }
            catch (Exception exc)
            {
                return String.Empty;
            }
        }

        public static List<ItemEx> DeserializeXmlToList(string listAsXml)
        {
            try
            {
                XmlSerializer xmlIzer = new XmlSerializer(typeof(List<ItemEx>));
                XmlReader xmlRead = XmlReader.Create(listAsXml);
                List<ItemEx> myList = new List<ItemEx>();
                myList = (xmlIzer.Deserialize(xmlRead)) as List<ItemEx>;
                return myList;
            }

            catch (Exception exc)
            {
                System.Diagnostics.Debug.WriteLine(exc);
                List<ItemEx> emptyList = new List<ItemEx>();
                return emptyList;
            }
        }

        public AppHelper()
        {
        }

        private static AppHelper _instance;

        public static AppHelper Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new AppHelper();
                return _instance;
            }
            private set
            {

            }
        }
    }
}
