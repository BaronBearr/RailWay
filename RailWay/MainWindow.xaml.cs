using RailWay.AllWindow;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace RailWay
{
    public partial class MainWindow : Window
    {
        private RegistrationWindow registrationWindow;
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RailwayTickets;Integrated Security=True";

        public MainWindow()
        {
            InitializeComponent();
        }

        private void TextBlock_MouseEnter(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Red;
            ((TextBlock)sender).TextDecorations = null;
        }

        private void TextBlock_MouseLeave(object sender, MouseEventArgs e)
        {
            ((TextBlock)sender).Foreground = Brushes.Blue;
            ((TextBlock)sender).TextDecorations = TextDecorations.Underline;
        }

        private void TextBlock_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (registrationWindow == null || !registrationWindow.IsVisible)
            {
                registrationWindow = new RegistrationWindow();
                registrationWindow.Show();
            }
            else
            {
                registrationWindow.Activate();
            }
        }

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
