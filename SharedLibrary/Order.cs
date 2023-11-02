namespace SharedLibrary
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public int BonNumber { get; set; } // Bonnumme
        public bool IsDelivery { get; set; } // Is delivered
        public string PaymentMethod { get; set; }
        public List<OrderItem> OrderItems { get; set; }

        public string LieferStatus
        {
            get { return IsDelivery ? "Lieferung" : "Abholung"; }
        }


        public Order()
        {
            OrderItems = new List<OrderItem>();
        }
    }
}