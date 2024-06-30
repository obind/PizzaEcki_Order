using System;
using Xunit;

namespace PizzaEcki.Test
{

    public class OrderServiceTests
    {
        private readonly OrderService _orderService;

        public OrderServiceTests()
        {
            _orderService = new OrderService();
        }

        [Theory]
        [InlineData("23:30", "18:00", "01:00", true)]   // Within overnight Happy Hour
        [InlineData("02:00", "18:00", "01:00", false)]  // Outside overnight Happy Hour
        [InlineData("12:00", "11:00", "14:00", true)]   // Within daytime Happy Hour
        [InlineData("10:30", "11:00", "14:00", false)]  // Before daytime Happy Hour
        [InlineData("14:00", "11:00", "14:00", true)]   // Exactly at the end of daytime Happy Hour
        [InlineData("18:00", "18:00", "23:00", true)]   // Exactly at the start of evening Happy Hour
        [InlineData("23:00", "18:00", "23:00", true)]   // Exactly at the end of evening Happy Hour
        [InlineData("23:01", "18:00", "23:00", false)]  // Just after evening Happy Hour
        public void TestIsHappyHour(string currentTimeStr, string happyHourStartStr, string happyHourEndStr, bool expected)
        {
            // Arrange
            TimeSpan currentTime = TimeSpan.Parse(currentTimeStr);
            TimeSpan happyHourStart = TimeSpan.Parse(happyHourStartStr);
            TimeSpan happyHourEnd = TimeSpan.Parse(happyHourEndStr);

            // Act
            bool result = _orderService.IsHappyHour(currentTime, happyHourStart, happyHourEnd);

            // Assert
            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData(DishCategory.Pizza, "L", "1", true)]   // Eligible Pizza during Happy Hour
        [InlineData(DishCategory.Pizza, "M", "1", false)]  // Non-eligible Pizza size during Happy Hour
        [InlineData(DishCategory.Nudeln, "M", "1", true)]  // Eligible Nudeln during Happy Hour
        [InlineData(DishCategory.Pasta, "L", "1", true)]   // Eligible Pasta during Happy Hour
        [InlineData(DishCategory.Burger, "L", "1", false)] // Non-eligible category during Happy Hour
        [InlineData(DishCategory.Pizza, "L", "0", false)]  // Non-Happy Hour
        public void TestIsEligibleForLunchOffer(DishCategory category, string size, string happyHour, bool expected)
        {
            // Arrange
            var dish = new Dish
            {
                Kategorie = category,
                HappyHour = happyHour
            };

            // Act
            bool result = _orderService.IsEligibleForLunchOffer(dish, size);

            // Assert
            Assert.Equal(expected, result);
        }
    }

}
