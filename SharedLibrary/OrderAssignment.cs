using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaServer
{
    public class OrderAssignment
    {
        [Key]
        public Guid OrderId { get; set; }
        public int BonNumber { get; set; }
        public int? DriverId { get; set; }
        public double? Price { get; set; }
        public DateTime Timestamp { get; set; }
    }
}
