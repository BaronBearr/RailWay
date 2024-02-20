using System;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using RailWay.AllClass;

namespace RailWay.AllPages
{
    public partial class PersonalAreaPage : Page
    {
        public PersonalAreaPage()
        {
            LoadData();
            InitializeComponent();
            UserGrid.DataContext = _user;
            CardGrid.DataContext = _card;
        }

        private User _user;
        private Card _card;

        private void LoadData()
        {
            int userId = ((MainWindow)Application.Current.MainWindow).UserID;
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query =
                        "select * from [User] as U join [Card] as C on U.CardId = C.CardID " +
                        "where U.UserId = @UserId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", userId);

                    SqlDataReader reader = command.ExecuteReader();
                    _user = new User();
                    _card = new Card();
                    _user.Card = _card;

                    while (reader.Read())
                    {
                        _user.FullName = reader["FullName"].ToString();
                        _user.Email = reader["Email"].ToString();
                        _user.Login = reader["Login"].ToString();
                        _user.Password = reader["Password"].ToString();
                        _user.Phone = reader["Phone"].ToString();
                        _card.Id = Convert.ToInt32(reader["CardID"]);
                        _card.Number = reader["Number"].ToString();
                        _card.Cvv = reader["Cvv"].ToString();
                        _card.ValidUntil = Convert.ToDateTime(reader["ValidUntil"]);
                        _card.Balance = (float)Convert.ToDouble(reader["Balance"]);
                    }
                    connection.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки даных пользователя: {ex.Message}");
            }
        }

        private bool CheckFields()
        {
            string email = EmailTextBox.Text;
            string login = LoginTextBox.Text;
            string password = PasswordTextBox.Text;
            float balance;
            DateTime validUntil = DatePicker.SelectedDate.Value;

            if (!String.IsNullOrWhiteSpace(email) &&
                !String.IsNullOrWhiteSpace(login) &&
                !String.IsNullOrWhiteSpace(password) &&
                validUntil != null &&
                PhoneTextBox.IsMaskCompleted &&
                CardNumberTextBox.IsMaskCompleted &&
                CardCvvTextBox.IsMaskCompleted &&
                !String.IsNullOrWhiteSpace(BalanceTextBox.Text))
            {
                string emailPattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$";
                if (!Regex.IsMatch(EmailTextBox.Text, emailPattern))
                {
                    MessageBox.Show("Некорректный формат электронной почты");
                    return false;
                }

                if (validUntil.Month <= DateTime.Today.Month && validUntil.Year <= DateTime.Today.Year)
                {
                    MessageBox.Show("Карта уже недействительна!");
                    return false;
                }


                bool balanceData = float.TryParse(BalanceTextBox.Text, out balance);
                if (!balanceData)
                {
                    MessageBox.Show("Неверный формат в поле Баланс!");
                    return false;
                }

                return true;
            }

            MessageBox.Show("Поля не могут быть пустыми");
            return false;
        }

        private void UpdateData()
        {
            try
            {
                if (_card.Id == 0)
                {
                    using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                    {
                        connection.Open();

                        string cardQuery = "insert into [Card] values (@Number, @Cvv, @ValidUntil, @Balance)";
                        SqlCommand cardCommand = new SqlCommand(cardQuery, connection);
                        cardCommand.Parameters.AddWithValue("@Number", CardNumberTextBox.Text);
                        cardCommand.Parameters.AddWithValue("@Cvv", CardCvvTextBox.Text);
                        cardCommand.Parameters.AddWithValue("@ValidUntil", DatePicker.SelectedDate);
                        cardCommand.Parameters.AddWithValue("@Balance", (float)Convert.ToDouble(BalanceTextBox.Text));

                        _card.Id = Convert.ToInt32(cardCommand.ExecuteScalar());
                        
                        connection.Close();
                    }
                }
                else
                {
                    using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                    {
                        connection.Open();

                        string cardQuery =
                            "update Card set Number = @Number, Cvv = @Cvv, ValidUntil = @ValidUntil, Balance = @Balance where CardID = @CardId";
                        SqlCommand cardCommand = new SqlCommand(cardQuery, connection);
                        cardCommand.Parameters.AddWithValue("@CardId", _card.Id);
                        cardCommand.Parameters.AddWithValue("@Number", CardNumberTextBox.Text);
                        cardCommand.Parameters.AddWithValue("@Cvv", CardCvvTextBox.Text);
                        cardCommand.Parameters.AddWithValue("@ValidUntil", DatePicker.SelectedDate);
                        cardCommand.Parameters.AddWithValue("@Balance", (float)Convert.ToDouble(BalanceTextBox.Text));
                        cardCommand.ExecuteNonQuery();
                        
                        connection.Close();
                    }
                }

                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string userQuery =
                        "update [User] set Email = @Email, [Login] = @Login, [Password] = @Password, CardId = @CardId where UserID = @UserId";
                    SqlCommand userCommand = new SqlCommand(userQuery, connection);
                    userCommand.Parameters.AddWithValue("@UserId", ((MainWindow)Application.Current.MainWindow).UserID);
                    userCommand.Parameters.AddWithValue("@Email", EmailTextBox.Text);
                    userCommand.Parameters.AddWithValue("@Login", LoginTextBox.Text);
                    userCommand.Parameters.AddWithValue("@Password", PasswordTextBox.Text);
                    userCommand.Parameters.AddWithValue("@CardId", _card.Id);
                    userCommand.ExecuteNonQuery();
                    
                    connection.Close();
                }
                
                MessageBox.Show("Изменения сохранены", "Удачно", MessageBoxButton.OK, MessageBoxImage.Asterisk);
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void SaveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (CheckFields())
            {
                UpdateData();
            }
        }
    }
}