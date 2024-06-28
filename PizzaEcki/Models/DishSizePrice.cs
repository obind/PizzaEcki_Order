using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class DishSizePrice
    {
        public string Size { get; set; }
        public double Price { get; set; }

        public override string ToString()
        {
            return $"{Size} - {Price:F2} €";
        }
    }

}
