using PizzaEcki.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Services
{
    public class OrderHelper
    {
        public bool IsHappyHour(DateTime currentDateTime, TimeSpan happyHourStart, TimeSpan happyHourEnd, DayOfWeek happyHourStartDay, DayOfWeek happyHourEndDay)
        {
            var currentTime = currentDateTime.TimeOfDay;
            var currentDay = currentDateTime.DayOfWeek;

            bool isValidDay = false;

            // Handle range of valid days
            if (happyHourStartDay <= happyHourEndDay)
            {
                isValidDay = currentDay >= happyHourStartDay && currentDay <= happyHourEndDay;
            }
            else
            {
                isValidDay = currentDay >= happyHourStartDay || currentDay <= happyHourEndDay;
            }

            if (!isValidDay)
            {
                return false;
            }

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

}
