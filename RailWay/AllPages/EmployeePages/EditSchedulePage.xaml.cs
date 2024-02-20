using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using RailWay.AllClass;

namespace RailWay.AllPages.EmployeePages
{
    public partial class EditSchedulePage : Page
    {
        private Train _train;
        List<TrainType> types = new List<TrainType>();
        List<Station> allStations = new List<Station>();
        List<Route> routes = new List<Route>();
        List<Station> stations = new List<Station>();

        public EditSchedulePage()
        {
            InitializeComponent();
            _train = new Train();
        }

        public EditSchedulePage(Train train)
        {
            InitializeComponent();

            _train = train;
            stations = new List<Station>();

            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string query = "select Station.Id as StationId, Station.Name as StationName from TrainStationRoute as TSR " +
                                   "join Station on TSR.StationId = Station.Id " +
                                   "where RouteId = @RouteId and TrainId = @TrainId";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@RouteId", _train.RouteId);
                    command.Parameters.AddWithValue("@TrainId", _train.TrainId);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Station station = new Station();
                        station.Id = Convert.ToInt32(reader["StationId"]);
                        station.Name = reader["StationName"].ToString();

                        stations.Add(station);
                    }

                    connection.Close();
                }

                StationDataGrid.ItemsSource = stations;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }
        }

        private void LoadData()
        {
            types = new List<TrainType>();
            allStations = new List<Station>();
            routes = new List<Route>();

            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query = "select * from TrainType";

                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        TrainType trainType = new TrainType();
                        trainType.Id = Convert.ToInt32(reader["TrainTypeID"]);
                        trainType.Name = reader["TypeName"].ToString();

                        types.Add(trainType);
                    }

                    connection.Close();
                    connection.Open();

                    query = "select * from Route";

                    command = new SqlCommand(query, connection);

                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Route route = new Route();
                        route.Id = Convert.ToInt32(reader["Id"]);
                        route.Name = reader["Name"].ToString();

                        routes.Add(route);
                    }

                    connection.Close();
                    connection.Open();
                    query = "select * from Station";

                    command = new SqlCommand(query, connection);

                    reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        Station station = new Station();
                        station.Id = Convert.ToInt32(reader["Id"]);
                        station.Name = reader["Name"].ToString();

                        allStations.Add(station);
                    }

                    connection.Close();
                }

                TrainTypeComboBox.ItemsSource = types;
                StationComboBox.ItemsSource = allStations;
                RouteComboBox.ItemsSource = routes;

                if (_train.TrainId != 0)
                {
                    RouteComboBox.SelectedItem = routes.First(c => c.Id == _train.RouteId);
                    TrainTypeComboBox.SelectedItem = types.First(c => c.Id == _train.TrainType.Id);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }
        }

        private void EditSchedulePage_OnLoaded(object sender, RoutedEventArgs e)
        {
            LoadData();
            DataContext = _train;
        }

        private void AddButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (StationComboBox.SelectedItem != null)
            {
                Station selectedStation = StationComboBox.SelectedItem as Station;
                Station tempStation = stations.FirstOrDefault(c => c.Id == selectedStation.Id);
                if (tempStation == null)
                {
                    stations.Add(selectedStation);
                    stations = stations.Distinct().ToList();
                    StationDataGrid.ItemsSource = null;
                    StationDataGrid.ItemsSource = stations;
                }
            }
            else
                MessageBox.Show("Выберите сначала станцию!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (StationDataGrid.SelectedItem != null)
            {
                Station selectedStation = StationDataGrid.SelectedItem as Station;
                Station tempStation = stations.FirstOrDefault(c => c.Id == selectedStation.Id);
                if (tempStation != null)
                {
                    stations.Remove(tempStation);
                    StationDataGrid.ItemsSource = null;
                    StationDataGrid.ItemsSource = stations;
                }
            }
            else
                MessageBox.Show("Выберите сначала станцию из таблицы!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private bool CheckFields()
        {
            if (TrainNumTextBox.Text != String.Empty &&
                TrainCostTextBox.Text != String.Empty &&
                TrainTypeComboBox.SelectedItem != null &&
                RouteComboBox.SelectedItem != null)
            {
                int num;
                int cost;
                if (stations.Count < 2)
                {
                    MessageBox.Show("Минимум 2 станции");
                    return false;
                }

                if (Int32.TryParse(TrainNumTextBox.Text, out num) &&
                    Int32.TryParse(TrainCostTextBox.Text, out cost))
                    return true;

                MessageBox.Show("Ошибка ввода данных в поля только для чисел");
                return false;
            }

            MessageBox.Show("Поля не могут быть пустыми");

            return false;
        }

        private void ButtonBase_OnClick(object sender, RoutedEventArgs e)
        {
            if (CheckFields())
            {
                try
                {
                    if (_train.TrainId != 0)
                    {
                        using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                        {
                            connection.Open();

                            string userQuery =
                                "update [Train] set TrainNum = @TrainNum, TicketCost = @TicketCost, TrainTypeId = @TrainTypeId where Id = @Id";
                            SqlCommand userCommand = new SqlCommand(userQuery, connection);
                            userCommand.Parameters.AddWithValue("@Id", _train.TrainId);
                            userCommand.Parameters.AddWithValue("@TrainNum", Convert.ToInt32(TrainNumTextBox.Text));
                            userCommand.Parameters.AddWithValue("@TicketCost", Convert.ToInt32(TrainCostTextBox.Text));
                            userCommand.Parameters.AddWithValue("@TrainTypeId", ((TrainType)TrainTypeComboBox.SelectedItem).Id);
                            userCommand.ExecuteNonQuery();

                            connection.Close();
                        }

                        using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                        {
                            connection.Open();

                            foreach (var item in stations)
                            {
                                string userQuery =
                                    "delete TrainStationRoute where TrainId = @TrainId and RouteId = @RouteId";
                                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                                userCommand.Parameters.AddWithValue("@TrainId", _train.TrainId);
                                userCommand.Parameters.AddWithValue("@RouteId", _train.RouteId);
                                userCommand.ExecuteNonQuery();
                            }

                            foreach (var item in stations)
                            {
                                string userQuery =
                                    "insert into TrainStationRoute values (@RouteId,@StationId,@TrainId)";
                                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                                userCommand.Parameters.AddWithValue("@RouteId", ((Route)RouteComboBox.SelectedItem).Id);
                                userCommand.Parameters.AddWithValue("@StationId", item.Id);
                                userCommand.Parameters.AddWithValue("@TrainId", _train.TrainId);
                                userCommand.ExecuteNonQuery();
                            }

                            connection.Close();
                        }

                        MessageBox.Show("Изменения сохранены", "Удачно", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                    else
                    {
                        using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                        {
                            connection.Open();

                            string userQuery =
                                "insert into [Train] values (@TrainNum,@TicketCost,@TrainTypeId)";
                            SqlCommand userCommand = new SqlCommand(userQuery, connection);
                            userCommand.Parameters.AddWithValue("@TrainNum", Convert.ToInt32(TrainNumTextBox.Text));
                            userCommand.Parameters.AddWithValue("@TicketCost", Convert.ToInt32(TrainCostTextBox.Text));
                            userCommand.Parameters.AddWithValue("@TrainTypeId", ((TrainType)TrainTypeComboBox.SelectedItem).Id);
                            _train.TrainId = Convert.ToInt32(userCommand.ExecuteScalar());

                            connection.Close();
                        }

                        using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                        {
                            connection.Open();

                            foreach (var item in stations)
                            {
                                string userQuery =
                                    "insert into TrainStationRoute values (@RouteId,@StationId,@TrainId)";
                                SqlCommand userCommand = new SqlCommand(userQuery, connection);
                                userCommand.Parameters.AddWithValue("@RouteId", ((Route)RouteComboBox.SelectedItem).Id);
                                userCommand.Parameters.AddWithValue("@StationId", item.Id);
                                userCommand.Parameters.AddWithValue("@TrainId", _train.TrainId);
                                userCommand.ExecuteNonQuery();
                            }

                            connection.Close();
                        }

                        MessageBox.Show("Изменения сохранены", "Удачно", MessageBoxButton.OK, MessageBoxImage.Asterisk);
                    }
                }
                catch (Exception exception)
                {
                    MessageBox.Show(exception.Message);
                }
            }
        }
    }
}