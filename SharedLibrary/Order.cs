using System.Text.Json.Serialization;

namespace SharedLibrary
{
    public class Order
    {
        [JsonPropertyName("orderId")]
        public Guid OrderId { get; set; }

        [JsonPropertyName("bonNumber")]
        public int BonNumber { get; set; }

        [JsonPropertyName("isDelivery")]
        public bool IsDelivery { get; set; }

        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; }

        [JsonPropertyName("orderItems")]
        public List<OrderItem> OrderItems { get; set; }

        [JsonPropertyName("phoneNumber")]
        public string CustomerPhoneNumber { get; set; }

        [JsonPropertyName("timestamp")]
        public string Timestamp { get; set; }

        [JsonPropertyName("deliveryUntil")]
        public string DeliveryUntil { get; set; }

        public string LieferStatus
        {
            get { return IsDelivery ? "Lieferung" : "Abholung"; }
        }

        public Customer Customer { get; set; }

        public Order()
        {
            OrderItems = new List<OrderItem>();
        }
    }
}