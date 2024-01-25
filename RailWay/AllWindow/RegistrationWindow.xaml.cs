using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RailWay.AllWindow
{
    public partial class RegistrationWindow : Window
    {
        private string connectionString = @"Data Source=DESKTOP-2MK3618\SQLEXPRESS02;Initial Catalog=RailwayTickets;Integrated Security=True";

        public RegistrationWindow()
        {
            InitializeComponent();
        }

        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string fullName = txtFullName.Text;
                DateTime dateOfBirth = dpDateOfBirth.SelectedDate ?? DateTime.MinValue;
                string email = txtEmail.Text;
                string gender = cmbGender.SelectedItem?.ToString();
                string login = txtLogin.Text;
                string password = txtPassword.Password; 
                string phone = txtPhone.Text;
                string passportNumber = txtPassportNumber.Text;
                DateTime expiryDate = dpExpiryDate.SelectedDate ?? DateTime.MinValue;
                DateTime issueDate = dpIssueDate.SelectedDate ?? DateTime.MinValue;
                string country = txtCountry.Text;
                string phonePattern = @"^\+7 \d{3} \d{3} \d{2} \d{2}$";
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                string passportFormat = @"^\d{4}\s\d{6}$";

                if (!Regex.IsMatch(passportNumber, passportFormat))
                {
                    MessageBox.Show("Некорректный номер паспорта");
                    return;
                }

                if (!Regex.IsMatch(email, emailPattern))
                {
                    MessageBox.Show("Некорректный формат электронной почты");
                    return;
                }

                if (Regex.IsMatch(fullName, @"\d"))
                {
                    MessageBox.Show("ФИО не может содержать цифры");
                    return;
                }
                if (Regex.IsMatch(country, @"\d"))
                {
                    MessageBox.Show("Страна не может содержать цифры");
                    return;
                }
                if (Regex.IsMatch(fullName, @"[!@#$%^&*()_+=\[{\]};:<>|./?,-]"))
                {
                    MessageBox.Show("ФИО не может содержать специальные символы");
                    return;
                }

                if (!Regex.IsMatch(phone, phonePattern))
                {
                    MessageBox.Show("Некорректный формат номера телефона. Пожалуйста, введите номер в формате +7 xxx xxx xx xx");
                    return;
                }

                if (dateOfBirth > DateTime.Now)
                {
                    MessageBox.Show("Дата рождения не может быть в будущем");
                    return;
                }

                if (issueDate > DateTime.Now)
                {
                    MessageBox.Show("Дата выдачи не может быть в будущем");
                    return;
                }

                if (dpDateOfBirth.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату рождения");
                    return;
                }

                if (dpExpiryDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату до какого годен паспорт");
                    return;
                }

                if (dpIssueDate.SelectedDate == null)
                {
                    MessageBox.Show("Выберите дату выдачи");
                    return;
                }

                if (cmbGender.SelectedIndex == -1)
                {
                    MessageBox.Show("Выберите ваш пол");
                    return;
                }

                if (string.IsNullOrWhiteSpace(login))
                {
                    MessageBox.Show("Введите логин");
                    return;
                }

                if (string.IsNullOrWhiteSpace(password))
                {
                    MessageBox.Show("Введите пароль");
                    return;
                }

                if (string.IsNullOrWhiteSpace(fullName))
                {
                    MessageBox.Show("Введите ФИО");
                    return;
                }

                if (string.IsNullOrWhiteSpace(email))
                {
                    MessageBox.Show("Введите почту");
                    return;
                }

                if (string.IsNullOrWhiteSpace(passportNumber))
                {
                    MessageBox.Show("Введите паспорт");
                    return;
                }

                if (string.IsNullOrWhiteSpace(country))
                {
                    MessageBox.Show("Введите страну");
                    return;
                }

                if (IsLoginExists(login))
                {
                    MessageBox.Show("Пользователь с таким логином уже существует.");
                    return;
                }

                if (string.IsNullOrWhiteSpace(phone))
                {
                    MessageBox.Show("Введите телефон");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(connectionString))
                {
                    connection.Open();

                    string passportQuery = "INSERT INTO PassportData (PassportNumber, ExpiryDate, IssueDate, Country, GenderID) VALUES (@PassportNumber, @ExpiryDate, @IssueDate, @Country, @GenderID); SELECT SCOPE_IDENTITY();";
                    SqlCommand passportCommand = new SqlCommand(passportQuery, connection);
                    passportCommand.Parameters.AddWithValue("@PassportNumber", passportNumber);
                    passportCommand.Parameters.AddWithValue("@ExpiryDate", expiryDate);
                    passportCommand.Parameters.AddWithValue("@IssueDate", issueDate);
                    passportCommand.Parameters.AddWithValue("@Country", country);
                    passportCommand.Parameters.AddWithValue("@GenderID", GetGenderId(gender));

                    int passportId = Convert.ToInt32(passportCommand.ExecuteScalar());

                    string userQuery = "INSERT INTO [User] (FullName, DoB, Email, Login, Password, Phone, PassportID, RoleID) VALUES (@FullName, @DoB, @Email, @Login, @Password, @Phone, @PassportID, @RoleID);";
                    SqlCommand userCommand = new SqlCommand(userQuery, connection);
                    userCommand.Parameters.AddWithValue("@FullName", fullName);
                    userCommand.Parameters.AddWithValue("@DoB", dateOfBirth);
                    userCommand.Parameters.AddWithValue("@Email", email);
                    userCommand.Parameters.AddWithValue("@Login", login);
                    userCommand.Parameters.AddWithValue("@Password", password);
                    userCommand.Parameters.AddWithValue("@Phone", phone);
                    userCommand.Parameters.AddWithValue("@PassportID", passportId);
                    userCommand.Parameters.AddWithValue("@RoleID", 2);

                    userCommand.ExecuteNonQuery();
                }

                MessageBox.Show("Пользователь успешно зарегистрирован!");
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации пользователя: {ex.Message}");
            }
        }

        private bool IsLoginExists(string login)
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM [User] WHERE Login = @Login";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@Login", login);

                int count = Convert.ToInt32(command.ExecuteScalar());

                return count > 0;
            }
        }

        private void LoadGenders()
        {
            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT GenderName FROM Gender";
                SqlCommand command = new SqlCommand(query, connection);

                try
                {
                    connection.Open();
                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        cmbGender.Items.Add(reader["GenderName"].ToString());
                    }

                    reader.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при загрузке данных: " + ex.Message);
                }
            }
        }

        private void genderComboBox_Loaded(object sender, RoutedEventArgs e)
        {
            LoadGenders();
        }

        private int GetGenderId(string genderName)
        {
            int genderId = -1;

            using (SqlConnection connection = new SqlConnection(connectionString))
            {
                string query = "SELECT GenderID FROM Gender WHERE GenderName = @GenderName";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@GenderName", genderName);

                try
                {
                    connection.Open();
                    var result = command.ExecuteScalar();

                    if (result != null && result != DBNull.Value)
                    {
                        genderId = Convert.ToInt32(result);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка при получении ID пола: " + ex.Message);
                }
            }

            return genderId;
        }

        private void PhoneNumberTextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (textBox.Text == "+7 xxx xxx xx xx")
            {
                textBox.Text = "";
                textBox.Foreground = Brushes.Black;
            }
        }

        private void PhoneNumberTextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            if (string.IsNullOrWhiteSpace(textBox.Text))
            {
                textBox.Text = "+7 xxx xxx xx xx";
                textBox.Foreground = Brushes.LightGray;
            }
        }

        private void PhoneNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (char.IsDigit(e.Text, 0) && textBox.Text.Length < 16)
            {
                if (textBox.Text.Length == 0)
                {
                    textBox.Text = "+7 ";
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else if (textBox.Text.Length == 6 || textBox.Text.Length == 10 || textBox.Text.Length == 13)
                {
                    textBox.Text += " " + e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else
                {
                    textBox.Text += e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            e.Handled = true;
        }

        private void PassportNumberTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            TextBox textBox = (TextBox)sender;

            if (char.IsDigit(e.Text, 0) && textBox.Text.Length < 11)
            {
                if (textBox.Text.Length == 4)
                {
                    textBox.Text += " " + e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
                else
                {
                    textBox.Text += e.Text;
                    textBox.CaretIndex = textBox.Text.Length;
                }
            }

            e.Handled = true;
        }

    }
}
