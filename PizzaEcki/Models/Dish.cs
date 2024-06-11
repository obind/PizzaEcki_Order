using PizzaEcki.Models;
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
        Salate,
        Baguette,
        Pasta,
        Nudeln,
        Döner,
        Omelette,
        Schnitzel,
        Kartoffelaufläufe, 
        TexMex,
        Extras,
        Getränke 

        // Weitere Kategorien können hier hinzugefügt werden
    }


    public class Dish
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public double Preis_S { get; set; }
        public double Preis_L { get; set; }
        public double Preis_XL { get; set; }
        public DishCategory Kategorie { get; set; }
        public string Größe { get; set; }
        public string HappyHour { get; set; }
        public double Steuersatz { get; set; }
        public int GratisBeilage { get; set; }

        public override string ToString()
        {
            return $"{Id} - " + $"{Name}";
        }
    }

}
