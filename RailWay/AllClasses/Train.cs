using System;

namespace RailWay.AllClass
{
    public class Train
    {
        public int TrainId { get; set; }
        public int TrainNumber { get; set; }
        public string RouteName { get; set; }
        public int RouteId { get; set; }
        public float TicketCost { get; set; }
        public TrainType TrainType { get; set;}
    }
}