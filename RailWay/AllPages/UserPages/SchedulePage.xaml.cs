using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using RailWay.AllClass;

namespace RailWay.AllPages
{
    public partial class SchedulePage : Page
    {
        public SchedulePage()
        {
            InitializeComponent();

            LoadRoutes();
            RouteComboBox.SelectedIndex = 0;
        }

        private void LoadRoutes()
        {
            List<Route> routes = new List<Route> { new Route() { Name = "Все" } };
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string query = "select * from Route";
                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        Route route = new Route
                        {
                            Id = Convert.ToInt32(reader["Id"].ToString()),
                            Name = reader["Name"].ToString()
                        };
                        routes.Add(route);
                    }

                    RouteComboBox.ItemsSource = routes;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки путей: {ex.Message}");
            }
        }

        private void TripsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => LoadTrains();

        private void LoadTrains()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query =
                        "select T.Id as TrainId, T.TrainNum, T.TicketCost,R.Name as RouteName,[Type].TrainTypeID, [Type].TypeName " +
                        "from TrainStationRoute as TSR " +
                        "join [Route] as R on TSR.RouteId = R.Id " +
                        "join Train as T on TSR.TrainId = T.Id " +
                        "join TrainType as [Type] on T.TrainTypeId = [Type].TrainTypeID ";
                    
                    if (RouteComboBox.SelectedIndex != 0)
                        query += " where R.Id = @RouteId";

                    query += " group by T.Id, T.TrainNum, T.TicketCost, R.Name, [Type].TypeName, [Type].TrainTypeID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RouteId", ((Route)RouteComboBox.SelectedItem).Id);

                    SqlDataReader reader = command.ExecuteReader();
                    List<Train> paths = new List<Train>();

                    while (reader.Read())
                    {
                        TrainType trainType = new TrainType();
                        trainType.Id = Convert.ToInt32(reader["TrainTypeID"]);
                        trainType.Name = reader["TypeName"].ToString();
                        Train train = new Train();
                        train.TrainId = Convert.ToInt32(reader["TrainId"]);
                        train.TrainNumber = Convert.ToInt32(reader["TrainNum"]);
                        train.TicketCost = (float)Convert.ToDouble(reader["TicketCost"]);
                        train.RouteName = reader["RouteName"].ToString();
                        train.TrainType = trainType;

                        paths.Add(train);
                    }

                    TrainsListView.ItemsSource = paths;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }
        }

        private void BuyTicketButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TrainsListView.SelectedItem != null)
            {
                MessageBoxResult result =
                    MessageBox.Show("Купить этот билет?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (((MainWindow)Application.Current.MainWindow).UserID != 0)
                    {
                        InsertData(TrainsListView.SelectedItem as Train, ((MainWindow)Application.Current.MainWindow).UserID);
                    }
                    else
                    {
                        result = MessageBox.Show(
                            "Для покупки билетов необходимо авторизоваться в системе. Перейти на страницу Авторизации?",
                            "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                            NavigationService.Navigate(new AuthPage());
                    }
                }
            }
            else
                MessageBox.Show("Выберите нужный поезд!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private bool CheckBalance(double cost)
        {
            try
            {
                string balance = String.Empty;

                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query = "select C.Balance from [User] as U join [Card] as C on U.CardId = C.CardID where U.UserID = @UserId";
                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", ((MainWindow)Application.Current.MainWindow).UserID);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        balance = reader["Balance"].ToString();
                    }
                }

                if (balance == String.Empty)
                {
                    var result = MessageBox.Show(
                        "К вашему аккаунту не привязана карта, вы можете это сделать в Личном Кабинете. Перейти на новую страницу?",
                        "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                    if (result == MessageBoxResult.Yes)
                        NavigationService.Navigate(new PersonalAreaPage());
                    return false;
                }

                if (Convert.ToDouble(balance) < cost)
                {
                    MessageBox.Show("На вашем счету недостаточно средств!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }

            return false;
        }

        private void InsertData(Train train, int userId)
        {
            if (!CheckBalance(train.TicketCost))
                return;

            using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string paymentQuery =
                        "INSERT INTO Payment  VALUES (@Amount, @PaymentDateTime, @PaymentMethodId); SELECT SCOPE_IDENTITY();";
                    SqlCommand paymentCommand = new SqlCommand(paymentQuery, connection);
                    paymentCommand.Parameters.AddWithValue("@Amount", train.TicketCost);
                    paymentCommand.Parameters.AddWithValue("@PaymentDateTime", DateTime.Now);
                    paymentCommand.Parameters.AddWithValue("@PaymentMethodId", 2);

                    int paymentId = Convert.ToInt32(paymentCommand.ExecuteScalar());

                    string balanceQuery = "update Card set Balance = (Balance - @Cost) where CardID " +
                                          "= (select C.CardID from [User] as U join [Card] as C on U.CardId = C.CardID where UserID = @UserId);";
                    SqlCommand balanceCommand = new SqlCommand(balanceQuery, connection);
                    balanceCommand.Parameters.AddWithValue("@Cost", train.TicketCost);
                    balanceCommand.Parameters.AddWithValue("@UserId", ((MainWindow)Application.Current.MainWindow).UserID);
                    balanceCommand.ExecuteNonQuery();

                    string ticketQuery =
                        "INSERT INTO Ticket VALUES " +
                        "(@DateCreate, @TrainID, @UserID, @PaymentID);";
                    SqlCommand userCommand = new SqlCommand(ticketQuery, connection);
                    userCommand.Parameters.AddWithValue("@DateCreate", DateTime.Now);
                    userCommand.Parameters.AddWithValue("@TrainID", train.TrainId);
                    userCommand.Parameters.AddWithValue("@UserID", userId);
                    userCommand.Parameters.AddWithValue("@PaymentID", paymentId);

                    userCommand.ExecuteNonQuery();
                    MessageBox.Show("Кулпенный билет вы можете посмотреть на странице ваших билетов!", "Сообщение", MessageBoxButton.OK,
                        MessageBoxImage.Asterisk);
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MyTicketsButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((MainWindow)Application.Current.MainWindow).UserID != 0)
                NavigationService.Navigate(new MyTicketsPage());
            else
            {
                var result = MessageBox.Show(
                    "Для просмотра купленных билетов необходимо авторизоваться в системе. Перейти на страницу Авторизации?",
                    "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    NavigationService.Navigate(new AuthPage());
            }
        }

        private void PersonalAreaButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (((MainWindow)Application.Current.MainWindow).UserID != 0)
                NavigationService.Navigate(new PersonalAreaPage());
            else
            {
                var result = MessageBox.Show(
                    "Для перехода в Личный Кабинет необходимо авторизоваться в системе. Перейти на страницу Авторизации?",
                    "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                if (result == MessageBoxResult.Yes)
                    NavigationService.Navigate(new AuthPage());
            }
        }
    }
}