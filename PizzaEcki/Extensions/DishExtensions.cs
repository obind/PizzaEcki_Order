using System.Collections.Generic;
using System.Linq;
using PizzaEcki.Models;

namespace PizzaEcki.Extensions
{
    public static class DishExtensions
    {
        public static List<Dish> SearchDishes(this List<Dish> dishes, string searchText)
        {
            return dishes.Where(d => d.Name.Contains(searchText)).ToList();
        }
    }
}

