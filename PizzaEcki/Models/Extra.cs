using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.ComponentModel;

namespace PizzaEcki.Models
{
    public class Extra : INotifyPropertyChanged
    {
        private int id;
        private string name;
        private double extraPreis_S;
        private double extraPreis_L;
        private double extraPreis_XL;

        public int Id
        {
            get { return id; }
            set { id = value; OnPropertyChanged(nameof(Id)); }
        }

        public string Name
        {
            get { return name; }
            set { name = value; OnPropertyChanged(nameof(Name)); }
        }

        public double ExtraPreis_S
        {
            get { return extraPreis_S; }
            set { extraPreis_S = value; OnPropertyChanged(nameof(ExtraPreis_S)); }
        }

        public double ExtraPreis_L
        {
            get { return extraPreis_L; }
            set { extraPreis_L = value; OnPropertyChanged(nameof(ExtraPreis_L)); }
        }

        public double ExtraPreis_XL
        {
            get { return extraPreis_XL; }
            set { extraPreis_XL = value; OnPropertyChanged(nameof(ExtraPreis_XL)); }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public override string ToString()
        {
            return $"{Name}";
        }
    }
}
