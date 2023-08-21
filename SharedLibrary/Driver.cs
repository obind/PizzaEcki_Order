﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SharedLibrary
{

    public class Driver
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string PhoneNumber { get; set; }

        // Überschreiben der ToString-Methode
        public override string ToString()
        {
            return Name;
        }
        
    }


}
