using PizzaServer;
using SharedLibrary;
using System;
namespace PizzaEcki.Models
{
public class Earnings
    {
        public DateTime Date { get; set; } // Das geparste Datum
        public string DateString { get; set; } // Das Datum als String
        public double TotalPrice { get; set; }
    }

}
