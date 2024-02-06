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

            LoadTripsComboBox();
            tripsComboBox.SelectedIndex = 0;
        }

        private void LoadTripsComboBox()
        {
            List<string> uniquePaths = new List<string> { "Все" };
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT DISTINCT " +
                                   "CONCAT(DepartureStation.StationName, ' - ', ArrivalStation.StationName) AS Path " +
                                   "FROM Train " +
                                   "JOIN Station AS DepartureStation ON Train.DepartureStationID = DepartureStation.StationID " +
                                   "JOIN Station AS ArrivalStation ON Train.ArrivalStationID = ArrivalStation.StationID";

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        string path = reader.GetString(0);
                        uniquePaths.Add(path);
                    }

                    tripsComboBox.ItemsSource = uniquePaths;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки путей: {ex.Message}");
            }
        }

        private void TripsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (tripsComboBox.SelectedItem != null)
            {
                string selectedPath = (string)tripsComboBox.SelectedItem;
                LoadPossiblePaths(selectedPath);
            }
        }

        private void LoadPossiblePaths(string selectedPath)
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query;
                    if (selectedPath == "Все")
                        query =
                            "SELECT TrainID," +
                            "TrainNumber," +
                            "Cost," +
                            "TrainType.TypeName," +
                            "DepartureStation.StationName as DStation, " +
                            "DepartureStationID, " +
                            "ArrivalStationID, " +
                            "ArrivalStation.StationName as AStation, " +
                            "DepartureTime, " +
                            "ArrivalTime, " +
                            "DepartureDate " +
                            "FROM Train " +
                            "JOIN Station AS DepartureStation ON Train.DepartureStationID = DepartureStation.StationID " +
                            "JOIN Station AS ArrivalStation ON Train.ArrivalStationID = ArrivalStation.StationID " +
                            "JOIN TrainType AS TrainType ON Train.TrainTypeId= TrainType.TrainTypeID";
                    else
                        query =
                            "SELECT TrainID," +
                            "TrainNumber," +
                            "Cost," +
                            "TrainType.TypeName," +
                            "DepartureStation.StationName as DStation, " +
                            "DepartureStationID, " +
                            "ArrivalStationID, " +
                            "ArrivalStation.StationName as AStation, " +
                            "DepartureTime, " +
                            "ArrivalTime, " +
                            "DepartureDate " +
                            "FROM Train " +
                            "JOIN Station AS DepartureStation ON Train.DepartureStationID = DepartureStation.StationID " +
                            "JOIN Station AS ArrivalStation ON Train.ArrivalStationID = ArrivalStation.StationID " +
                            "JOIN TrainType AS TrainType ON Train.TrainTypeId= TrainType.TrainTypeID " +
                            "WHERE CONCAT(DepartureStation.StationName, ' - ', ArrivalStation.StationName) = @SelectedPath";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@SelectedPath", selectedPath);

                    SqlDataReader reader = command.ExecuteReader();
                    List<TrainPath> paths = new List<TrainPath>();

                    while (reader.Read())
                    {
                        TrainPath trainPath = new TrainPath();
                        trainPath.TrainId = Convert.ToInt32(reader["TrainID"]);
                        trainPath.TrainNumber = Convert.ToInt32(reader["TrainNumber"]);
                        trainPath.Cost = (float)Convert.ToDouble(reader["Cost"]);
                        trainPath.TrainType = reader["TypeName"].ToString();
                        trainPath.DepartureStation = reader["DStation"].ToString();
                        trainPath.DepartureStationId = Convert.ToInt32(reader["DepartureStationID"]);
                        trainPath.ArrivalStationId = Convert.ToInt32(reader["ArrivalStationID"]);
                        trainPath.ArrivalStation = reader["AStation"].ToString();
                        trainPath.DepartureTime = (TimeSpan)reader["DepartureTime"];
                        trainPath.ArrivalTime = (TimeSpan)reader["ArrivalTime"];
                        trainPath.DepartureDate = (DateTime)reader["DepartureDate"];

                        paths.Add(trainPath);
                    }

                    TicketsDataGrid.ItemsSource = paths;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }
        }

        private void SchedulePage_OnLoaded(object sender, RoutedEventArgs e) => LoadTripsComboBox();

        private void BuyTicketButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TicketsDataGrid.SelectedItem != null)
            {
                MessageBoxResult result =
                    MessageBox.Show("Купить этот билет?", "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);
                if (result == MessageBoxResult.Yes)
                {
                    if (((MainWindow)Application.Current.MainWindow).UserID != 0)
                    {
                        InsertData(TicketsDataGrid.SelectedItem as TrainPath, ((MainWindow)Application.Current.MainWindow).UserID);
                    }
                    else
                    {
                        result = MessageBox.Show(
                            "Для просмотра купленных билетов необходимо авторизоваться в системе. Перейти на страницу Авторизации?",
                            "Сообщение", MessageBoxButton.YesNo, MessageBoxImage.Question);

                        if (result == MessageBoxResult.Yes)
                            NavigationService.Navigate(new AuthPage());
                    }
                }
            }
            else
                MessageBox.Show("Выберите нужный поезд!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void InsertData(TrainPath trainPath, int userId)
        {
            using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
            {
                try
                {
                    connection.Open();

                    string paymentQuery =
                        "INSERT INTO Payment  VALUES (@Amount, @PaymentDateTime, @PaymentMethodId); SELECT SCOPE_IDENTITY();";
                    SqlCommand paymentCommand = new SqlCommand(paymentQuery, connection);
                    paymentCommand.Parameters.AddWithValue("@Amount", trainPath.Cost);
                    paymentCommand.Parameters.AddWithValue("@PaymentDateTime", DateTime.Now);
                    paymentCommand.Parameters.AddWithValue("@PaymentMethodId", 2);

                    int paymentId = Convert.ToInt32(paymentCommand.ExecuteScalar());

                    string ticketQuery =
                        "INSERT INTO Ticket VALUES " +
                        "(@DepartureCity, @ArrivalCity, @DepartureDate, @DepartureTime, @ArrivalTime, @DepartureStationID, @ArrivalStationID, @UserID, @StatusID,@TrainID,@PaymentID);";
                    SqlCommand userCommand = new SqlCommand(ticketQuery, connection);
                    userCommand.Parameters.AddWithValue("@DepartureCity", trainPath.DepartureStation);
                    userCommand.Parameters.AddWithValue("@ArrivalCity", trainPath.ArrivalStation);
                    userCommand.Parameters.AddWithValue("@DepartureDate", trainPath.DepartureDate);
                    userCommand.Parameters.AddWithValue("@DepartureTime", trainPath.DepartureTime);
                    userCommand.Parameters.AddWithValue("@ArrivalTime", trainPath.ArrivalTime);
                    userCommand.Parameters.AddWithValue("@DepartureStationID", trainPath.DepartureStationId);
                    userCommand.Parameters.AddWithValue("@ArrivalStationID", trainPath.ArrivalStationId);
                    userCommand.Parameters.AddWithValue("@UserID", userId);
                    userCommand.Parameters.AddWithValue("@StatusID", 1);
                    userCommand.Parameters.AddWithValue("@TrainID", trainPath.TrainId);
                    userCommand.Parameters.AddWithValue("@PaymentID", paymentId);

                    userCommand.ExecuteNonQuery();
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message, "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        private void MyTicketsButton_OnClick(object sender, RoutedEventArgs e)
        {
        }
    }
}