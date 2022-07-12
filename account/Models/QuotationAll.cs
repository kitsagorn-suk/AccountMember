using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class QuotationAll
    {
        public int id { get; set; } = 0;
        public string no { get; set; } = "";
        public int month { get; set; } = 0;
        public int year { get; set; } = 0;
        public decimal grand_total { get; set; } = 0;
        public int company_id { get; set; } = 0;
        public string name { get; set; } = "";
        public int company_prefix_id { get; set; } = 0;
        public string name_prefix { get; set; } = "";
        public int status { get; set; } = 0;
        public decimal complete_amount { get; set; } = 0;
        public decimal balance_amount { get; set; } = 0;
        public decimal overpay { get; set; } = 0;
        public string bank { get; set; } = "";
        public bool is_confirm { get; set; } = false;
        public string start { get; set; } = "";
        public string end { get; set; } = "";
    }
}