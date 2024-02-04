using RailWay.AllClass;
using RailWay.AllClasses;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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

namespace RailWay.AllPages
{
    public partial class AddSchedule : Page
    {
        public ObservableCollection<Station> Stations { get; set; }
        public AddSchedule()
        {
            InitializeComponent();
            Stations = LoadStationsFromDatabase();

            cmbDepartureStation.ItemsSource = Stations;
            cmbArrivalStation.ItemsSource = Stations;
        }

        private ObservableCollection<Station> LoadStationsFromDatabase()
        {
            ObservableCollection<Station> stations = new ObservableCollection<Station>();

            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string query = "SELECT StationID, StationName FROM Station";
                    SqlCommand command = new SqlCommand(query, connection);

                    SqlDataReader reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        int stationID = reader.GetInt32(0);
                        string stationName = reader.GetString(1);

                        stations.Add(new Station { StationID = stationID, StationName = stationName });
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке станций: {ex.Message}");
            }

            return stations;
        }

        private void AddTrainButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string trainNumber = txtTrainNumber.Text;
                int departureStationID = (int)cmbDepartureStation.SelectedValue;
                int arrivalStationID = (int)cmbArrivalStation.SelectedValue;
                TimeSpan departureTime = TimeSpan.Parse(txtDepartureTime.Text);
                TimeSpan arrivalTime = TimeSpan.Parse(txtArrivalTime.Text);

                if (!IsNumeric(txtTrainNumber.Text))
                {
                    MessageBox.Show("Номер поезда должен содержать только цифры");
                    return;
                }

                if (departureStationID == arrivalStationID)
                {
                    MessageBox.Show("Станция отправления не может совпадать с конечной станцией");
                    return;
                }

                if (IsTrainNumberExists(trainNumber))
                {
                    MessageBox.Show("Поезд с таким номером уже существует");
                    return;
                }

                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();

                    string query = "INSERT INTO Train (TrainNumber, DepartureStationID, ArrivalStationID, DepartureTime, ArrivalTime) " +
                                   "VALUES (@TrainNumber, @DepartureStationID, @ArrivalStationID, @DepartureTime, @ArrivalTime)";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@TrainNumber", trainNumber);
                    command.Parameters.AddWithValue("@DepartureStationID", departureStationID);
                    command.Parameters.AddWithValue("@ArrivalStationID", arrivalStationID);
                    command.Parameters.AddWithValue("@DepartureTime", departureTime);
                    command.Parameters.AddWithValue("@ArrivalTime", arrivalTime);

                    command.ExecuteNonQuery();
                }

                MessageBox.Show("Поезд успешно добавлен");
                
                ClearFields();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при добавлении поезда: {ex.Message}");
            }
        }

        private void ClearFields()
        {
            txtTrainNumber.Text = string.Empty;
            cmbDepartureStation.SelectedIndex = -1;
            cmbArrivalStation.SelectedIndex = -1;
            txtDepartureTime.Text = string.Empty;
            txtArrivalTime.Text = string.Empty;
        }



        private bool IsTrainNumberExists(string trainNumber)
        {
            using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
            {
                connection.Open();

                string query = "SELECT COUNT(*) FROM Train WHERE TrainNumber = @TrainNumber";
                SqlCommand command = new SqlCommand(query, connection);
                command.Parameters.AddWithValue("@TrainNumber", trainNumber);

                int count = (int)command.ExecuteScalar();

                return count > 0;
            }
        }

        private bool IsNumeric(string value)
        {
            return int.TryParse(value, out _);
        }

    }
}
