using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace HouseholdBank.Models
{
    public class Kredi
    {
        public int krediMiktari { get; set; }
        public int yas { get; set; }
        public int aldigi_kredi_sayi { get; set; }
        public string evDurumu { get; set; }
        public string telefonDurumu { get; set; }
    }
}