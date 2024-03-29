﻿using System;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using RailWay.AllClass;

namespace RailWay.AllPages
{
    public partial class AuthPage : Page
    {
        public AuthPage()
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

        private void LoginButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string enteredLogin = txtUsername.Text;
                string enteredPassword = txtPassword.Password;

                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
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

                        ((MainWindow)Application.Current.MainWindow).UserID = userId;
                        switch (roleId)
                        {
                            case 1:
                                NavigationService.Navigate(new EmployeePages.SсhedulePage());
                                break;
                            case 2:
                                NavigationService.Navigate(new SchedulePage());
                                break;
                        }
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

        private void GuestLoginButton_OnClick(object sender, RoutedEventArgs e)
        {
            ((MainWindow)Application.Current.MainWindow).UserID = 0;
            NavigationService.Navigate(new SchedulePage());
        }
    }
}