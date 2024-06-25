using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class PaymentMethodSummary
    {
        public string PaymentMethod { get; set; }
        public int OrderCount { get; set; }
        public double TotalSales { get; set; }
    }
}
