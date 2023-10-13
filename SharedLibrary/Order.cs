namespace SharedLibrary
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public int BonNumber { get; set; } // Bonnummer
        public string PaymentMethod { get; set; }
        public List<OrderItem> OrderItems { get; set; }


        public Order()
        {
            OrderItems = new List<OrderItem>();
        }
    }
}