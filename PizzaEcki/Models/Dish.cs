using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ConstrainedExecution;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Xps.Serialization;
using System.Xml.Linq;

namespace PizzaEcki.Models
{

    public enum DishCategory
    {
        Pizza,
        Salad,
        Noodles,
        Other,
        // Weitere Kategorien können hier hinzugefügt werden
    }

    public class Dish
    {
            public int Id { get; set; }
            public string Name { get; set; }
            public double Price { get; set; }
            public DishCategory Category { get; set; }
            public string Size { get; set; } = "L";

        public override string ToString()
            {
                return $"{Id} - " + $"{Name}";
            }
    }

}
