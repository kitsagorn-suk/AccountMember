using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class Treatment
    {
        public string _id { get; set; } = "";
        public int discount { get; set; } = 0;
        public int vat { get; set; } = 0;
        public string createDate { get; set; } = "";
        public string updateDate { get; set; } = "";
        public string prefix { get; set; } = "";
        public string prefixType { get; set; } = "";
        public string billNo { get; set; } = "";
        public string date { get; set; } = "";
        public float total { get; set; } = 0;
        public float subTotal { get; set; } = 0;
        public float grandTotal { get; set; } = 0;
        public string granTotalText { get; set; } = "";
        public string status { get; set; } = "";
        public int pageTotal { get; set; } = 0;
        public Bank bank { get; set; }
        public Customer customer { get; set; }
        public List<Transactions> transactions { get; set; }
    }

    public class Bank
    {
        public string bankName { set; get; } = "";
        public string accountName { set; get; } = "";
        public string accountNo { set; get; } = "";
        public string bankShortName { set; get; } = "";
    }

    public class Customer
    {
        public int id { set; get; } = 0;
        public string company { set; get; } = "";
        public string companyType { set; get; } = "";
        public string tel { set; get; } = "";
        public string fax { set; get; } = "";

    }
    

    public class Transactions
    {
        public string _id { set; get; } = "";
        public string id { set; get; } = "";
        public string description { set; get; } = "";
        public int qty { set; get; } = 0;
        public string unit { set; get; } = "";
        public float unitPrice { set; get; } = 0;
        public float amount { set; get; } = 0;

    }
}