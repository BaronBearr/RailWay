using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using RailWay.AllClass;

namespace RailWay.AllPages.EmployeePages
{
    public partial class SсhedulePage : Page
    {
        public SсhedulePage()
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

        private void LoadTrains()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query =
                        "select T.Id as TrainId, T.TrainNum, T.TicketCost,R.Name as RouteName,R.Id as RouteId,[Type].TrainTypeID, [Type].TypeName " +
                        "from TrainStationRoute as TSR " +
                        "join [Route] as R on TSR.RouteId = R.Id " +
                        "join Train as T on TSR.TrainId = T.Id " +
                        "join TrainType as [Type] on T.TrainTypeId = [Type].TrainTypeID ";

                    if (RouteComboBox.SelectedIndex != 0)
                        query += " where R.Id = @RouteId";

                    query += " group by T.Id, T.TrainNum, T.TicketCost, R.Name, [Type].TypeName, [Type].TrainTypeID, R.Id";

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
                        train.RouteId = Convert.ToInt32(reader["RouteId"]);
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

        private void RouteComboBox_OnSelectionChanged(object sender, SelectionChangedEventArgs e) => LoadTrains();

        private void AddTrainButton_OnClick(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EditSchedulePage());
        }

        private void EditTrainButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (TrainsListView.SelectedItem != null)
            {
                Train train = TrainsListView.SelectedItem as Train;
                NavigationService.Navigate(new EditSchedulePage(train));
            }
            else
                MessageBox.Show("Выберите сначала запись!", "Сообщение", MessageBoxButton.OK, MessageBoxImage.Stop);
        }
    }
}