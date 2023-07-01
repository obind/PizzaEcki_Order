using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class OrderItem
    {
        private static int currentMaxId = 0;   

        public OrderItem() {

            Nr = ++currentMaxId;
        }  


        public int Nr { get; set; }
        public string Gericht { get; set; }
        public string Extras { get; set; }
        public int Menge { get; set; }
        public double Epreis { get; set; }
        public double Gesamt { get; set; }

        public string Uhrzeit { get; set; }

        // ... weitere Eigenschaften und Methoden nach Bedarf
    }

}
