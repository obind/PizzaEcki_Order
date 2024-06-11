using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace SharedLibrary
{
    public class OrderItem : INotifyPropertyChanged
    {
        private static int currentMaxId = 0;

        private int _orderItemId;
        private Guid _orderId;
        private string _gericht;
        private string _größe;
        private string _extras;
        private int _menge;
        private double _epreis;
        private double _gesamt;
        private string _uhrzeit;
        private int _lieferungsArt;
        private int _nr;

        public OrderItem()
        {
            _nr = ++currentMaxId;
        }

        [Key]
        [JsonPropertyName("orderItemId")]
        public int OrderItemId
        {
            get => _orderItemId;
            set => SetProperty(ref _orderItemId, value);
        }

        [JsonPropertyName("orderId")]
        public Guid OrderId
        {
            get => _orderId;
            set => SetProperty(ref _orderId, value);
        }

        [JsonPropertyName("gericht")]
        public string Gericht
        {
            get => _gericht;
            set => SetProperty(ref _gericht, value);
        }

        [JsonPropertyName("größe")]
        public string Größe
        {
            get => _größe;
            set => SetProperty(ref _größe, value);
        }

        [JsonPropertyName("extras")]
        public string Extras
        {
            get => _extras;
            set => SetProperty(ref _extras, value);
        }

        [JsonPropertyName("menge")]
        public int Menge
        {
            get => _menge;
            set => SetProperty(ref _menge, value);
        }

        [JsonPropertyName("epreis")]
        public double Epreis
        {
            get => _epreis;
            set => SetProperty(ref _epreis, value);
        }

        [JsonPropertyName("gesamt")]
        public double Gesamt
        {
            get => _gesamt;
            set => SetProperty(ref _gesamt, value);
        }

        [JsonPropertyName("uhrzeit")]
        public string Uhrzeit
        {
            get => _uhrzeit;
            set => SetProperty(ref _uhrzeit, value);
        }

        [JsonPropertyName("lieferungsArt")]
        public int LieferungsArt
        {
            get => _lieferungsArt;
            set => SetProperty(ref _lieferungsArt, value);
        }

        [NotMapped]
        public int Nr
        {
            get => _nr;
            set => SetProperty(ref _nr, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void SetProperty<T>(ref T storage, T value, [CallerMemberName] string propertyName = null)
        {
            if (Equals(storage, value)) return;
            storage = value;
            OnPropertyChanged(propertyName);
        }

        protected void OnPropertyChanged(string propertyName) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
