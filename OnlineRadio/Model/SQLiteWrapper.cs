using SQLite;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnlineRadio.Model
{
    public class SQLiteWrapper
    {
        private SQLiteWrapper()
        {
            dbPath = Path.Combine(Windows.Storage.ApplicationData.Current.LocalFolder.Path, _dbFileName);            
        }
        private static string _dbFileName = "db.sqlite";
        private static string dbPath;

        private bool tablePresent = false;
        private static SQLiteWrapper _instance;
        public static SQLiteWrapper Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SQLiteWrapper();
                }
                return _instance;
            }

            private set
            {
                _instance = value;
            }
        }

        public async Task<bool> CreateTableIfNot()
        {
            tablePresent = await DatabaseHelper.Instance.FileExists();
            if (tablePresent == false)
            {
                tablePresent = DatabaseHelper.Instance.CreateTable();
            }
            return tablePresent;
        }

        public List<ItemEx> GetItemsFromTableUsingCategory(string category)
        {
            List<ItemEx> allItems = new List<ItemEx>();
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where category = '{0}'", category);
                    var existing = db.Query<ItemEx>(query);
                    allItems.AddRange(existing);
                }
            }
            catch (Exception e)
            {

            }
            return allItems;
        }

        public List<ItemEx> GetAllItems()
        {
            List<ItemEx> allItems = new List<ItemEx>();
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx");
                    var existing = db.Query<ItemEx>(query);
                    allItems.AddRange(existing);
                }
            }
            catch (Exception e)
            {

            }
            return allItems;
        }

        public List<ItemEx> GetItemsFromTableUsingId(string id)
        {
            List<ItemEx> allItems = new List<ItemEx>();
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where chlink = '{0}'", id);
                    var existing = db.Query<ItemEx>(query);
                    allItems.AddRange(existing);
                }
            }
            catch (Exception e)
            {

            }
            return allItems;
        }

        public List<ItemEx> GetRecentChannels()
        {
            List<ItemEx> allItems = new List<ItemEx>();
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where IsRecent = 1");
                    var existing = db.Query<ItemEx>(query);
                    allItems.AddRange(existing);
                    allItems = allItems.OrderByDescending(o => o.RecentIndex).ToList();
                }
            }
            catch (Exception e)
            {

            }
            return allItems;
        }

        public List<ItemEx> GetFavChannels()
        {
            List<ItemEx> allItems = new List<ItemEx>();
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where IsFav = 1");
                    var existing = db.Query<ItemEx>(query);
                    allItems.AddRange(existing);
                    allItems = allItems.OrderByDescending(o => o.RecentIndex).ToList();
                }
            }
            catch (Exception e)
            {

            }
            return allItems;
        }

        public bool UpdateItemInTable(Item item)
        {
            bool updated = false;
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where chlink = '{0}'", item.chlink);
                    var existing = db.Query<ItemEx>(query).FirstOrDefault();
                    if (existing == null)
                    {
                        db.RunInTransaction(() =>
                        {
                            ItemEx newItem = new ItemEx();
                            newItem.category = item.category;
                            newItem.chcat = item.chcat;
                            newItem.chdescription = item.chdescription;
                            newItem.chlink = item.chlink;
                            newItem.chtype = item.chtype;
                            newItem.title = item.title;
                            db.Insert(newItem, typeof(ItemEx));
                            updated = true;
                        });
                    }
                    else
                    {
                        db.RunInTransaction(() =>
                        {
                            try
                            {
                                existing.category = item.category;
                                existing.chcat = item.chcat;
                                existing.chdescription = item.chdescription;
                                if (existing.chlink != item.chlink)
                                {
                                    existing.chlink = item.chlink;
                                }
                                existing.chtype = item.chtype;
                                existing.title = item.title;
                                db.Update(existing, typeof(ItemEx));
                                updated = true;
                            }
                            catch (Exception ex)
                            {
                                Debug.WriteLine("venkat:" + ex.ToString());
                            }
                        });
                    }
                    db.Commit();
                    db.Close();
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine("venkat1: " + e.ToString());
            }
            return updated;
        }

        public bool UpdateItemExInTable(ItemEx item)
        {
            bool updated = false;
            try
            {
                using (var db = new SQLiteConnection(dbPath))
                {
                    string query = string.Format("select * from ItemEx where chlink = '{0}'", item.chlink);
                    var existing = db.Query<ItemEx>(query).FirstOrDefault();
                    if (existing == null)
                    {
                        db.RunInTransaction(() =>
                        {
                            ItemEx newItem = new ItemEx();
                            newItem.category = item.category;
                            newItem.chcat = item.chcat;
                            newItem.chdescription = item.chdescription;
                            newItem.chlink = item.chlink;
                            newItem.chtype = item.chtype;
                            newItem.title = item.title;
                            db.Insert(newItem, typeof(ItemEx));
                            updated = true;
                        });
                    }
                    else
                    {
                        db.RunInTransaction(() =>
                        {
                            try
                            {
                                existing.category = item.category;
                                existing.chcat = item.chcat;
                                existing.chdescription = item.chdescription;
                                existing.chlink = item.chlink;
                                existing.chtype = item.chtype;
                                existing.title = item.title;
                                existing.IsFav = item.IsFav;
                                existing.IsRecent = item.IsRecent;
                                existing.RecentIndex = item.RecentIndex;
                                db.Update(existing, typeof(ItemEx));
                                updated = true;
                            }
                            catch (Exception ex)
                            {

                            }
                        });
                    }
                    db.Commit();
                    db.Close();
                }
            }
            catch (Exception e)
            {

            }
            return updated;
        }
    }
}
