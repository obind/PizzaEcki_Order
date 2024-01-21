using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    // OrderSummary class definition
    public class OrderSummary
    {
        public string OrderType { get; set; }
        public int Count { get; set; }
        public double Total { get; set; }
    }

}
