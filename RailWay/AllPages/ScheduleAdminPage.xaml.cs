using RailWay.AllClass;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Globalization;
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

namespace RailWay.AllPages
{
    public partial class ScheduleAdminPage : Page
    {
        public ScheduleAdminPage()
        {
            InitializeComponent();
            LoadTripsComboBox();
        }
        private void LoadTripsComboBox()
        {
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
                    List<string> uniquePaths = new List<string>();

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

                    string query = "SELECT DepartureStation.StationName, ArrivalStation.StationName, DepartureTime, ArrivalTime " +
                                   "FROM Train " +
                                   "JOIN Station AS DepartureStation ON Train.DepartureStationID = DepartureStation.StationID " +
                                   "JOIN Station AS ArrivalStation ON Train.ArrivalStationID = ArrivalStation.StationID " +
                                   "WHERE CONCAT(DepartureStation.StationName, ' - ', ArrivalStation.StationName) = @SelectedPath";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@SelectedPath", selectedPath);

                    SqlDataReader reader = command.ExecuteReader();
                    List<string> possiblePaths = new List<string>();

                    while (reader.Read())
                    {
                        string departureStation = reader.GetString(0);
                        string arrivalStation = reader.GetString(1);
                        TimeSpan departureTime = reader.GetTimeSpan(2);
                        TimeSpan arrivalTime = reader.GetTimeSpan(3);

                        string path = $"{departureStation} - {arrivalStation} ({departureTime} - {arrivalTime})";
                        possiblePaths.Add(path);
                    }

                    ticketsListBox.ItemsSource = possiblePaths;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки возможных путей: {ex.Message}");
            }
        }

        private void AddSchedule_Click(object sender, EventArgs e)
        {
            NavigationService.Navigate(new AddSchedule());
        }

        private void DeleteSchedule_Click(object sender, RoutedEventArgs e)
        {
           
        }


       


    }
}
