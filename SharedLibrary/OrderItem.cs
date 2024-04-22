using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
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
        [JsonPropertyName("orderItemId")]
        public int OrderItemId { get; set; }
        
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }


        [JsonPropertyName("gericht")]
        public string Gericht { get; set; }

        [JsonPropertyName("größe")]
        public string Größe { get; set; }

        [JsonPropertyName("extras")]
        public string? Extras { get; set; }

        [JsonPropertyName("menge")]
        public int Menge { get; set; }

        [JsonPropertyName("epreis")]
        public double Epreis { get; set; }

        [JsonPropertyName("gesamt")]
        public double Gesamt { get; set; }

        [JsonPropertyName("uhrzeit")]
        public string? Uhrzeit { get; set; }

        [JsonPropertyName("lieferungsArt")]
        public int LieferungsArt { get; set; }

        // ... weitere Eigenschaften und Methoden nach Bedarf
        [NotMapped]
        public int Nr { get; set; }
    }

}
