﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace PizzaEcki.Models
{
    public class DailySalesInfo
    {
        public string Name { get; set; }
        public int Count { get; set; }

        public double DailySales { get; set; }
        public string PaymentMethod { get; set; }
    }

}
