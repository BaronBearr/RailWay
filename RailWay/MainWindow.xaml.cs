using System.Windows;
using System.Windows.Navigation;
using RailWay.AllPages;

namespace RailWay
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            Frame.Navigate(new AuthPage());
        }

        private void Frame_OnNavigated(object sender, NavigationEventArgs e)
        {
            if (Frame.CanGoBack)
            {
                BackButton.Visibility = Visibility.Visible;
            }
            else
            {
                BackButton.Visibility = Visibility.Collapsed;
            }
        }

        private void BackButton_OnClick(object sender, RoutedEventArgs e)
        {
            Frame.GoBack();
        }
    }
}