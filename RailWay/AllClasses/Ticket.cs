using System;

namespace RailWay.AllClass
{
    public class Ticket
    {
        public DateTime DateCreate { get; set; }
        public string PaymentMethod { get; set; }
        public Train Train { get; set; }
    }
}