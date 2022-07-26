using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class PayHistory
    {
        public int Id { get; set; } = 0;
        public string bill_trans { get; set; } = "";
        public string currency_name { get; set; } = "";
        public decimal rate { get; set; } = 0;
        public decimal amount { get; set; } = 0;
        public decimal amount_thai { get; set; } = 0;
        public string create_date { get; set; } = "";
        public string file_code { get; set; } = "";
        public string url { get; set; } = "";
        public int account_type { get; set; } = 0;
        public string my { get; set; } = "";
    }
}