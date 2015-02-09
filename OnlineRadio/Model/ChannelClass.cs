using GalaSoft.MvvmLight;
using SQLite;
using System.Collections.Generic;

public class Item :ViewModelBase
{
    public string title { get; set; }
    [PrimaryKey]
    public string chlink { get; set; }
    public string chdescription { get; set; }
    public string chcat { get; set; }
    public string chtype { get; set; }
    public string category { get; set; }
}

public class Channel
{
    public string title { get; set; }
    public string link { get; set; }
    public string description { get; set; }
    public List<Item> item { get; set; }
}

public class Rss
{
    //public string __invalid_name__-version { get; set; }
    public Channel channel { get; set; }
}

public class RootObject
{
    public Rss rss { get; set; }
}
[Table("ItemEx")]
public class ItemEx : Item
{
    private bool _isFav;
    public bool IsFav
    {
        get
        {
            return _isFav;
        }
        set
        {
            _isFav = value;
            RaisePropertyChanged("IsFav");
        }
    }

    private bool _isRecent;
    public bool IsRecent
    {
        get
        {
            return _isRecent;
        }
        set
        {
            _isRecent = value;
            RaisePropertyChanged("IsRecent");
        }
    }

    public int RecentIndex { get; set; }
    public override bool Equals(object obj)
    {
        var newItem = obj as ItemEx;
        if (this.chlink == newItem.chlink)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    [Ignore]
    public string DisplayTitle
    {
        get
        {
            if(string.IsNullOrEmpty(category))
            {
                return title;                
            }
            else
            {
                return title + " - " + category;
            }
        }
        private set
        {

        }
    }
}
