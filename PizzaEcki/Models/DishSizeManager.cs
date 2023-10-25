using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    internal class DishSizeManager
    {
        public static Dictionary<DishCategory, List<string>> CategorySizes = new Dictionary<DishCategory, List<string>>
        {
            { DishCategory.Pizza, new List<string> { "S", "M", "L", "XL" } },
            { DishCategory.Salat, new List<string> { "S", "L" } },
            { DishCategory.Nudeln, new List<string> { "L" } },
            {DishCategory.Döner, new List<string> { "L",} }
            // Weitere Kategorien und Größen können hier hinzugefügt werden
        };
    }
}
