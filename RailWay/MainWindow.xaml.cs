using RailWay.AllWindow;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
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
            try
            {
                string enteredLogin = txtUsername.Text;
                string enteredPassword = txtPassword.Password;

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string query = "SELECT UserID, RoleID FROM [User] WHERE Login = @Login AND Password = @Password";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@Login", enteredLogin);
                    command.Parameters.AddWithValue("@Password", enteredPassword);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        int userId = reader.GetInt32(0);
                        int roleId = reader.GetInt32(1);

                        UserWindow userWindow = new UserWindow(userId, roleId);
                        userWindow.Show();
                        this.Close();
                    }
                    else
                    {
                        MessageBox.Show("Неверный логин или пароль.");
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при авторизации: {ex.Message}");
            }
        }
    }
}
