using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRadio.Model
{
    public class DatabaseHelper
    {
        private string _dbFileName = "db.sqlite";
        private string dbPath;
        private bool isFilePresent = false;
        private DatabaseHelper()
        {
            dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dbFileName);
        }

        private static DatabaseHelper _instance;

        public static DatabaseHelper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new DatabaseHelper();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }
        public async Task<bool> FileExists()
        {
            if (!isFilePresent)
            {
                try
                {
                    var store = await Windows.Storage.ApplicationData.Current.LocalFolder.GetFileAsync(_dbFileName);
                    isFilePresent = true;
                }
                catch
                {
                }
            }
            return isFilePresent;
        }

        public bool CreateTable()
        {
            bool created = false;
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    db.CreateTable<ItemEx>(CreateFlags.ImplicitPK);                    
                    created = true;
                }
            }
            catch (Exception e)
            {
                created = false;
            }
            return created;
        }
    }
}
