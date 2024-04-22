using SharedLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class OrderChangedEventArgs : EventArgs
    {
        public Order Order { get; set; }
    }
}
