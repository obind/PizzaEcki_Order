using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Serialization;
using System.Xml.Linq;

namespace PizzaEcki.Models
{
    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Price { get; set; }


        public override string ToString()
        {
           return $"{Id} - " + $"{Name}";
        }

    }

}
