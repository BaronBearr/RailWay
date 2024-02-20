using System;

namespace RailWay.AllClass
{
    public class Card
    {
        public int Id { get; set; }
        public string Number { get; set; }
        public string Cvv { get; set; }
        public DateTime ValidUntil { get; set; }
        
        public float Balance { get; set; } 
    }
}