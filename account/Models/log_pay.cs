using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class log_pay
    {
        public int Id { get; set; } = 0;
        public int actionID { get; set; } = 0;
        public string actionName { get; set; } = "";
        public string fileExtension { get; set; } = "";
        public string name { get; set; } = "";
        public string url { get; set; } = "";
        public string bill_trans { get; set; } = "";
        public string createBy { get; set; } = "";
        public string fileCode { get; set; } = "";
        public string SlipImg { get; set; } = "";
        public int compId { get; set; } = 0;

        public string billtrans { get; set; } = "";
        public int currencyId { get; set; } = 0;
        public decimal rate { get; set; } = 0;
        public decimal amount { get; set; } = 0;
        public int accountType { get; set; } = 0;
    }
}