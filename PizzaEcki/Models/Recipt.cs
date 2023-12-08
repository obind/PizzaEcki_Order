using PizzaEcki.Models;
using System.Collections.Generic;
using System.Linq;
using SharedLibrary;

public class Receipt
{
    public int ReceiptNumber { get; set; }
    public List<OrderItem> OrderItems { get; set; }
    public double TotalPrice => OrderItems.Sum(item => item.Gesamt);
}
