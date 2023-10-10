using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class OrderAssignment
    {
        public int BonNumber { get; set; }
        public int DriverId { get; set; }
        public double Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
