using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Test
{
    public class OrderService
    {
        public bool IsHappyHour(TimeSpan currentTime, TimeSpan happyHourStart, TimeSpan happyHourEnd)
        {
            // Handle overnight happy hours
            if (happyHourStart > happyHourEnd)
            {
                return currentTime >= happyHourStart || currentTime <= happyHourEnd;
            }

            return currentTime >= happyHourStart && currentTime <= happyHourEnd;
        }

        public bool IsEligibleForLunchOffer(Dish selectedDish, string selectedSize)
        {
            return (selectedDish.Kategorie == DishCategory.Pizza && selectedSize == "L" && selectedDish.HappyHour == "1") ||
                   (selectedDish.Kategorie == DishCategory.Nudeln && selectedDish.HappyHour == "1") ||
                   (selectedDish.Kategorie == DishCategory.Pasta && selectedDish.HappyHour == "1");
        }
    }

    public class Dish
    {
        public DishCategory Kategorie { get; set; }
        public string HappyHour { get; set; }
    }

    public enum DishCategory
    {
        Pizza,
        Nudeln,
        Pasta,
        Burger
    }

}
