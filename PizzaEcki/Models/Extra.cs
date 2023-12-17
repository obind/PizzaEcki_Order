using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class Extra
    {

        public int Id { get; set; }
        public string Name { get; set; }
        public double ExtraPreis_S { get; set; }
        public double ExtraPreis_L { get; set; }
        public double ExtraPreis_XL { get; set; }

        public override string ToString()
        {
            return $"{Name}";
        }


    }
}
