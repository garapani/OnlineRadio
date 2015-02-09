using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Data;

namespace OnlineRadio.Converters
{
    public sealed class StringToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string culture)
        {            

            if (value == null)
            {
                return false;
            }
            else
            {
                string temp = value as string;
                if (string.IsNullOrEmpty(temp))
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string culture)
        {
            throw new NotImplementedException();
        }
    }
}
