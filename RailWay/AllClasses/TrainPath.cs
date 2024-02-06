using System;

namespace RailWay.AllClass
{
    public class TrainPath
    {
        public int TrainId { get; set; }
        public int TrainNumber { get; set; }
        public int DepartureStationId { get; set; }
        public string DepartureStation { get; set; }
        public int ArrivalStationId { get; set; }
        public string ArrivalStation { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public DateTime DepartureDate { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public string TrainType { get; set; }
        public float Cost { get; set; }

        public string Stations => DepartureStation + " - " + ArrivalStation;
        public string Time => DepartureTime + " - " + ArrivalTime;
    }
}