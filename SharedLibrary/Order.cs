using System.ComponentModel;
using System.Text.Json.Serialization;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace SharedLibrary
{
    public class Order : INotifyPropertyChanged
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

        [JsonPropertyName("driverId")]
        public int? DriverId { get; set; }

        private string _name;
        public string Name
        {
            get => _name;
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged();
                }
            }
        }
        public string LieferStatus
        {
            get { return IsDelivery ? "Lieferung" : "Abholung"; }
        }

        private Customer _customer;
        public Customer Customer
        {
            get => _customer;
            set
            {
                if (_customer != value)
                {
                    _customer = value;
                    OnPropertyChanged();
                }
            }
        }


        public Order()
        {
            OrderItems = new List<OrderItem>();
        }

        public double Gesamtpreis => OrderItems.Sum(item => item.Gesamt);

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}