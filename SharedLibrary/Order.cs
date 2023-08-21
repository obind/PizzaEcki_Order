namespace SharedLibrary
{
    public class Order
    {
        public Guid OrderId { get; set; }
        public int BonNumber { get; set; } // Bonnummer
        public List<OrderItem> OrderItems { get; set; }
    }
}