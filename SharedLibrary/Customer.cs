using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SharedLibrary
{
   public class Customer
   {
        [Key]
        public string PhoneNumber { get; set; }
     
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("street")]
        public string Street { get; set; }
        
        [JsonPropertyName("city")]
        public string City { get; set; }

        public string AdditionalInfo { get; set; }
    }

}
