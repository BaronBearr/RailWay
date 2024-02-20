using System.Windows;
using System.Windows.Navigation;
using RailWay.AllPages;
using RailWay.AllPages.EmployeePages;

namespace RailWay
{
    public partial class MainWindow : Window
    {
        public int UserID;
        public MainWindow()
        {
            InitializeComponent();
            // Frame.Navigate(new AuthPage());
            Frame.Navigate(new SсhedulePage());
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