using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace SharedLibrary
{
    public class OrderItem
    {
        private static int currentMaxId = 0;   

        public OrderItem() {

            Nr = ++currentMaxId;
        }
        [Key]
        public int OrderItemId { get; set; }
        public string Gericht { get; set; }
        public string Größe { get; set; }
        public string? Extras { get; set; }
        public int Menge { get; set; }
        public double Epreis { get; set; }
        public double Gesamt { get; set; }

        public string? Uhrzeit { get; set; }
        public int LieferungsArt { get; set; }

        // ... weitere Eigenschaften und Methoden nach Bedarf
        [NotMapped]
        public int Nr { get; set; }
    }

}
