using System;
using Xunit;

public class LunchOfferTests
{
    [Fact]
    public void TestIsEligibleForLunchOffer_PizzaL_HappyHour()
    {
        // Arrange: Setup the test data
        var selectedDish = new Dish
        {
            Kategorie = DishCategory.Pizza,
            HappyHour = "1"
        };
        var selectedSize = "L";

        // Act: Call the method to test
        var result = IsEligibleForLunchOffer(selectedDish, selectedSize);

        // Assert: Verify the result
        Assert.True(result);
    }

    [Fact]
    public void TestIsEligibleForLunchOffer_Nudeln_HappyHour()
    {
        // Arrange
        var selectedDish = new Dish
        {
            Kategorie = DishCategory.Nudeln,
            HappyHour = "1"
        };
        var selectedSize = "M";

        // Act
        var result = IsEligibleForLunchOffer(selectedDish, selectedSize);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TestIsEligibleForLunchOffer_NotEligible()
    {
        // Arrange
        var selectedDish = new Dish
        {
            Kategorie = DishCategory.Burger,
            HappyHour = "0"
        };
        var selectedSize = "L";

        // Act
        var result = IsEligibleForLunchOffer(selectedDish, selectedSize);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void TestIsHappyHour_WithinRange()
    {
        // Arrange
        var currentTime = new TimeSpan(23, 30, 0);
        var happyHourStart = new TimeSpan(18, 0, 0);
        var happyHourEnd = new TimeSpan(1, 0, 0);

        // Act
        var result = IsHappyHour(currentTime, happyHourStart, happyHourEnd);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void TestIsHappyHour_OutsideRange()
    {
        // Arrange
        var currentTime = new TimeSpan(14, 0, 0);
        var happyHourStart = new TimeSpan(18, 0, 0);
        var happyHourEnd = new TimeSpan(1, 0, 0);

        // Act
        var result = IsHappyHour(currentTime, happyHourStart, happyHourEnd);

        // Assert
        Assert.False(result);
    }

    private bool IsEligibleForLunchOffer(Dish selectedDish, string selectedSize)
    {
        return (selectedDish.Kategorie == DishCategory.Pizza && selectedSize == "L" && selectedDish.HappyHour == "1") ||
               (selectedDish.Kategorie == DishCategory.Nudeln && selectedDish.HappyHour == "1") ||
               (selectedDish.Kategorie == DishCategory.Pasta && selectedDish.HappyHour == "1");
    }

    private bool IsHappyHour(TimeSpan currentTime, TimeSpan happyHourStart, TimeSpan happyHourEnd)
    {
        // Handle overnight happy hours
        if (happyHourStart > happyHourEnd)
        {
            return currentTime >= happyHourStart || currentTime <= happyHourEnd;
        }

        return currentTime >= happyHourStart && currentTime <= happyHourEnd;
    }

    private class Dish
    {
        public DishCategory Kategorie { get; set; }
        public string HappyHour { get; set; }
    }

    private enum DishCategory
    {
        Pizza,
        Nudeln,
        Pasta,
        Burger
    }
}
