using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Windows;
using System.Windows.Controls;
using RailWay.AllClass;

namespace RailWay.AllPages
{
    public partial class MyTicketsPage : Page
    {
        public MyTicketsPage()
        {
            InitializeComponent();
            LoadTickets();
        }

        private void LoadTickets()
        {
            try
            {
                using (SqlConnection connection = new SqlConnection(DBBase.ConnectionString))
                {
                    connection.Open();
                    string query =
                        "SELECT Tk.DateCreate,T.Id, T.TrainNum, T.TicketCost, R.Name as RouteName, PM.MethodName as PaymentMethod, Tp.TypeName, Tp.TrainTypeID " +
                        "FROM [Ticket] AS Tk " +
                        "join Train as T on Tk.TrainId = T.Id " +
                        "JOIN [TrainStationRoute] AS TSR ON Tk.TrainId = TSR.TrainId " +
                        "JOIN [Route] AS R ON TSR.RouteId = R.Id " +
                        "JOIN [Payment] AS P ON Tk.PaymentId = P.PaymentID " +
                        "join TrainType as Tp on T.TrainTypeId = Tp.TrainTypeID " +
                        "join PaymentMethod as PM on P.PaymentMethodID = PM.PaymentMethodID " +
                        "where Tk.UserId = @UserId " +
                        "group by Tk.DateCreate,T.Id, T.TrainNum, R.Name, PM.MethodName, T.TicketCost, Tp.TypeName, Tp.TrainTypeID";

                    SqlCommand command = new SqlCommand(query, connection);
                    command.Parameters.AddWithValue("@UserId", ((MainWindow)Application.Current.MainWindow).UserID);

                    SqlDataReader reader = command.ExecuteReader();
                    List<Ticket> tickets = new List<Ticket>();

                    while (reader.Read())
                    {
                        TrainType trainType = new TrainType();
                        trainType.Id = Convert.ToInt32(reader["TrainTypeID"]);
                        trainType.Name = reader["TypeName"].ToString();
                        Train train = new Train();
                        train.TrainId = Convert.ToInt32(reader["Id"]);
                        train.TrainNumber = Convert.ToInt32(reader["TrainNum"]);
                        train.TicketCost = (float)Convert.ToDouble(reader["TicketCost"]);
                        train.RouteName = reader["RouteName"].ToString();
                        train.TrainType = trainType;
                        Ticket ticket = new Ticket();
                        ticket.DateCreate = Convert.ToDateTime(reader["DateCreate"]);
                        ticket.PaymentMethod = reader["PaymentMethod"].ToString();
                        ticket.Train = train;

                        tickets.Add(ticket);
                    }

                    TrainsListView.ItemsSource = tickets;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки билетов: {ex.Message}");
            }
        }
    }
}