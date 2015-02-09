using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Core;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace OnlineRadio.Common
{
    public class LayoutAwarePage : Page
    {
        public LayoutAwarePage()
        {
            Window.Current.SizeChanged += WindowSizeChanged;
            Loaded += LayoutAwarePage_Loaded;
        }

        private void LayoutAwarePage_Loaded(object sender, RoutedEventArgs e)
        {
            SetVisualState();
        }

        private void WindowSizeChanged(object sender, WindowSizeChangedEventArgs e)
        {
            SetVisualState();
        }


        public void SetVisualState()
        {
            string viewValue = ApplicationView.Value.ToString();
            VisualStateManager.GoToState(this, viewValue, false);
        }
    }
}
