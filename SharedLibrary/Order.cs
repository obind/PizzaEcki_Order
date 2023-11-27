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