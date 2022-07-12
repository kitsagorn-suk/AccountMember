using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class BankList
    {
        public int id { get; set; } = 0;
        public string bank { get; set; } = "";
        public int type { get; set; } = 0;
        public string account { get; set; } = "";
        public string accountName { get; set; } = "";
    }
}