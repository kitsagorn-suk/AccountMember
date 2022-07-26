using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using account.Models;
using account.Database;
using System.Globalization;
using System.Data.Entity.Validation;
using CrystalDecisions.CrystalReports.Engine;
using CrystalDecisions.ReportAppServer.DataDefModel;
using CrystalDecisions.Shared;
using ConnectionInfo = CrystalDecisions.Shared.ConnectionInfo;
using PagedList;
using System.Data.SqlClient;
using System.Configuration;
using System.Data;
using static account.Models.HomeViewModel;
using System.Web.Configuration;
using System.Drawing;

namespace account.Controllers
{
    public class QuotationController : Controller
    {

        //DropDown : SelectListItem Define for Month & Year DropDownList.  
        List<SelectListItem> ddlMonths = new List<SelectListItem>();
        List<SelectListItem> ddlYears = new List<SelectListItem>();

        private SelectList GetYears(int? iSelectedYear)
        {
            int CurrentYear = DateTime.Now.Year;
            int rangYear = CurrentYear - 5;
            
            for (int i = rangYear; i <= CurrentYear; i++)
            {
                ddlYears.Add(new SelectListItem
                {
                    Text = i.ToString(),
                    Value = i.ToString()
                });
            }
            ddlYears.Add(new SelectListItem
            {
                Text = "--Select--",
                Value = "0"
            });
            //Default It will Select Current Year  
            return new SelectList(ddlYears.OrderBy(x=>x.Value), "Value", "Text", iSelectedYear);

        }

        private SelectList GetMonths(int? iSelectedYear)
        {
            var months = Enumerable.Range(1, 12).Select(i => new
            {
                A = i,
                B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
            });

            int CurrentMonth = 12; //January  

            if (iSelectedYear == DateTime.Now.Year)
            {
                months = Enumerable.Range(1, CurrentMonth).Select(i => new
                {
                    A = i,
                    B = DateTimeFormatInfo.CurrentInfo.GetMonthName(i)
                });
            }
            ddlMonths.Add(new SelectListItem { Text = "--Select--", Value = "0" });
            foreach (var item in months)
            {
                ddlMonths.Add(new SelectListItem { Text = item.B.ToString(), Value = item.A.ToString() });
            }

            //Default It will Select Current Month  
            return new SelectList(ddlMonths, "Value", "Text", DateTime.Now.Month);

        }

        // GET: Quotation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Menu()
        {
            return View();
        }

        public ActionResult HistoryList(int id, int? page,int? Year)
        {
            if (Year == null)
            {
                Year = DateTime.Now.Year;
            }

            ViewBag.linktoYearId = GetYears(Year);
            ViewBag.linktoMonthId = GetMonths(Year);
            accountEntities db = new accountEntities();
            var _comp = db.user_member_login.Where(x => x.id == id).FirstOrDefault();
            //var data = db.log_pay_member.Where(y => y.create_by == id).ToList();

            List<PayHistory> payHistoryList = new List<PayHistory>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT lm.id, lm.bill_transaction_id, sc.name, lm.rate, lm.amount, lm.amount_thai, CONVERT(VARCHAR, lm.create_date, 103) AS create_date, lm.file_code";
            query += " ,Stuff(";
            query += " (";
            query += " Select ',' + b.no";
            query += " From bill_transaction As b";
            query += " Where b.id IN(SELECT * FROM dbo.splitstring(lm.bill_transaction_id))";
            query += " For Xml Path('')";
            query += " ), 1, 1, '') As bill_no";
            query += " FROM log_pay_member AS lm";
            query += " LEFT JOIN system_currency AS sc ON lm.currency_id = sc.id";
            query += " WHERE lm.status = 1 and lm.company_id = " + _comp.company_id;
           // query += " GROUP BY lm.file_code, lm.bill_transaction_id, CONVERT(VARCHAR, lm.create_date, 103), lm.create_by";


            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var payHis = new PayHistory();
                    var _my = "";
                    payHis.Id = Convert.ToInt32(rdr["id"]);
                    payHis.bill_trans = rdr["bill_no"].ToString();
                    payHis.file_code = rdr["file_code"].ToString();
                    payHis.currency_name = rdr["name"].ToString();
                    payHis.rate = Convert.ToDecimal(rdr["rate"]);
                    payHis.amount = Convert.ToDecimal(rdr["amount"]);
                    payHis.amount_thai = Convert.ToDecimal(rdr["amount_thai"]);
                    payHis.create_date = rdr["create_date"].ToString();

                    if (rdr["create_date"].ToString().Substring(3, 1) == "0")
                    {
                        _my =  rdr["create_date"].ToString().Substring(6)+ rdr["create_date"].ToString().Substring(4, 1);
                    }
                    else
                    {
                        _my =  rdr["create_date"].ToString().Substring(6)+ rdr["create_date"].ToString().Substring(3, 2);
                    }
                   
                    payHis.my = _my;
                    payHistoryList.Add(payHis);
                }
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(payHistoryList.ToPagedList(pageNumber, pageSize));
        }
        
        public RedirectResult Quotations(int Id)
        {
            return Redirect("/viewQuotation.aspx?id=" + Id);

        }

        public RedirectResult QuotationsPaid(int Id)
        {
            return Redirect("/viewQuotationPaid.aspx?id=" + Id);
        }

        public ActionResult ListQuotation(int id, int? page, int? Year)
        {
            if (Year == null)
            {
                Year = DateTime.Now.Year;
            }

            ViewBag.linktoYearId = GetYears(Year);
            ViewBag.linktoMonthId = GetMonths(Year);


            int iMonth = 0, iYear = 0;
            iMonth = DateTime.Now.Month;
            iYear = DateTime.Now.Year;
            //list from create because comnyid in system_user = 1 all
            var db = new accountEntities();
            var _comp = db.user_member_login.Where(x => x.id == id).FirstOrDefault();

            List<QuotationAll> quotationList = new List<QuotationAll>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT bt.id,bt.month, bt.year, bt.company_id, c.name, bt.status,bt.system_bank_id, SUM(grand_total) grand_total,";
            query += " SUM(btd.balance_amount) balance_amount, SUM(btd.complete_amount) complete_amount, SUM(btd.overpay) overpay";
            query += " FROM bill_transaction bt";
            query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount,";
            query += " SUM(complete_amount) +SUM(overpay) complete_amount, SUM(overpay) overpay";
            query += " FROM bill_transaction_detail";
            query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
            query += " LEFT JOIN company c ON bt.company_id = c.id";
            query += " WHERE bt.month = "+ iMonth + " and bt.year = "+ iYear + " and bt.company_id = " + _comp.company_id;
            query += " GROUP BY  bt.id, bt.month, bt.year, bt.status, bt.company_id, c.name, bt.system_bank_id";
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var quotation = new QuotationAll();

                    quotation.id = Convert.ToInt32(rdr["id"]);
                    //quotation.no = rdr["no"].ToString();
                    
                    quotation.month = Convert.ToInt32(rdr["month"]);
                    quotation.year = Convert.ToInt32(rdr["year"]);
                    quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                    quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                    quotation.name = Convert.ToString(rdr["name"]);
                    quotation.status = Convert.ToInt32(rdr["status"]);
                    quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                    quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                    quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                    quotationList.Add(quotation);
                }
            }
            
            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(quotationList.ToPagedList(pageNumber, pageSize));
        }

        public ActionResult ListPrefix(int id,string date, int? page)
        {
           
            var year = date.Substring(0, 4);
            var month = date.Substring(4);

            List<QuotationAll> quotationList = new List<QuotationAll>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT bt.id, bt.no, bt.month, bt.year, bt.company_id, bt.status, bt.system_bank_id, bt.grand_total, ";
            query += " btd.balance_amount, btd.complete_amount, btd.overpay ,c.name ,cp.name name_prefix ,bt.start_date,bt.end_date";
            query += " FROM bill_transaction AS bt";
            query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount, SUM(complete_amount) +SUM(overpay) complete_amount,";
            query += " SUM(overpay) overpay FROM bill_transaction_detail";
            query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
            query += " LEFT JOIN company c ON bt.company_id = c.id";
            query += " LEFT JOIN company_prefix cp ON bt.company_prefix_id = cp.id";
            query += " WHERE bt.month = "+month+" AND bt.year = "+year+" AND bt.company_id = "+id;
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var quotation = new QuotationAll();

                    quotation.id = Convert.ToInt32(rdr["id"]);
                    quotation.no = rdr["no"].ToString();
                    quotation.month = Convert.ToInt32(rdr["month"]);
                    quotation.year = Convert.ToInt32(rdr["year"]);
                    quotation.bank = rdr["system_bank_id"].ToString();
                    quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                    quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                    quotation.name = Convert.ToString(rdr["name"]);
                    quotation.status = Convert.ToInt32(rdr["status"]);
                    quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                    quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                    quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                    quotation.start = Convert.ToDateTime(rdr["start_date"]).ToString("dd/MM/yyyy");
                    quotation.end = Convert.ToDateTime(rdr["end_date"]).ToString("dd/MM/yyyy");
                    quotationList.Add(quotation);
                }
            }

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(quotationList.ToPagedList(pageNumber, pageSize));
        }

        public JsonResult getNameCompany(int id)
        {
            var db = new accountEntities();
            var _user = db.user_member_login.Where(x => x.id == id).FirstOrDefault();
            var comp = db.companies.Where(y => y.id == _user.company_id).FirstOrDefault();
            var compID = comp.name;
            
            ViewBag.company = compID;
            return Json(new { success = ViewBag.company }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getBank(string LBank)
        {

            List<BankList> bankList = new List<BankList>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT s.id, s.bank_id, s.bank_type,";
            query += " s.bank_acc_no account_no, s.bank_acc_name account_name, bc.full_name_th bank_name, bc.short_code";
            query += " FROM system_bank s";
            query += " LEFT JOIN bank_code bc ON s.bank_id = bc.id";
            query += " WHERE s.id IN("+ LBank + ") ";

            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var bank = new BankList();
                    
                    bank.bank = rdr["bank_name"].ToString();
                    bank.type = Convert.ToInt32(rdr["bank_type"]);
                    bank.account = rdr["account_no"].ToString();
                    bank.accountName = rdr["account_name"].ToString();
                   
                    bankList.Add(bank);
                }
            }
            return Json(new { result = bankList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult getSilp(string fileCode)
        {

            List<PayHistory> PayList = new List<PayHistory>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT url";
            query += " FROM file_detail";
            query += " where file_code = '"+fileCode+"' and status = 1";
           

            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var pay = new PayHistory();
                    
                    pay.url = rdr["url"].ToString();

                    PayList.Add(pay);
                }
            }
            return Json(new { result = PayList }, JsonRequestBehavior.AllowGet);
        }

        public JsonResult SearchQuotationNo(string quo, string date, int id)
        {
            var year = date.Substring(0, 4);
            var month = date.Substring(4);
            var _quo = quo.ToUpper();

            accountEntities db = new accountEntities();
            var Data = db.bill_transaction.ToList();
            try
            {
                var comp = db.user_member_login.Where(y => y.id == id).FirstOrDefault();
               // Data = db.bill_transaction.Where(x => x.month == month && x.year == year && x.company_id == comp.company_id).ToList();

                List<QuotationAll> quotationList = new List<QuotationAll>();
                string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                string query = "SELECT bt.id, bt.no, bt.month, bt.year, bt.company_id, bt.status, bt.system_bank_id, bt.grand_total,  btd.balance_amount, btd.complete_amount, btd.overpay ,c.name ,cp.name name_prefix ,bt.start_date,bt.end_date";
                query += " FROM bill_transaction AS bt";
                query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount, SUM(complete_amount) +SUM(overpay) complete_amount, SUM(overpay) overpay";
                query += " FROM bill_transaction_detail";
                query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
                query += " LEFT JOIN company c ON bt.company_id = c.id";
                query += " LEFT JOIN company_prefix cp ON bt.company_prefix_id = cp.id";
                query += " WHERE bt.no LIKE '%"+ _quo + "%' and month = " + month + " and year = " + year + " AND bt.company_id = " + id;
                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var quotation = new QuotationAll();

                        quotation.id = Convert.ToInt32(rdr["id"]);
                        quotation.no = rdr["no"].ToString();
                        quotation.month = Convert.ToInt32(rdr["month"]);
                        quotation.year = Convert.ToInt32(rdr["year"]);
                        quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                        quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                        quotation.name = Convert.ToString(rdr["name"]);
                        quotation.status = Convert.ToInt32(rdr["status"]);
                        quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                        quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                        quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                        quotation.start = Convert.ToDateTime(rdr["start_date"]).ToString("dd/MM/yyyy");
                        quotation.end = Convert.ToDateTime(rdr["end_date"]).ToString("dd/MM/yyyy");
                        quotation.name_prefix = rdr["name_prefix"].ToString();
                        quotation.bank = rdr["system_bank_id"].ToString();
                        quotationList.Add(quotation);
                    }
                }

                var json = JsonConvert.SerializeObject(Data);

                return Json(new { result = quotationList, message = "request successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request failed" });
            }
        }

        public JsonResult SearchQuotationNoHistory(string quo, string month,string year, int id)
        {
           
            var _quo = quo.ToUpper();

            accountEntities db = new accountEntities();
            var Data = db.bill_transaction.ToList();
            try
            {
                var comp = db.user_member_login.Where(y => y.id == id).FirstOrDefault();
                List<PayHistory> payHistoryList = new List<PayHistory>();
                string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                string _query = "SELECT * FROM";
                _query += " (SELECT lm.id, lm.bill_transaction_id, sc.name, lm.rate, lm.amount, lm.amount_thai, CONVERT(VARCHAR, lm.create_date, 103) AS create_date, lm.file_code";
                _query += " ,Stuff(( Select ',' + b.no ";
                _query += " From bill_transaction As b";
                _query += " Where b.id IN(SELECT * FROM dbo.splitstring(lm.bill_transaction_id))";
                _query += " For Xml Path('')";
                _query += " ), 1, 1, '') As bill_no";
                _query += " FROM log_pay_member AS lm";
                _query += " LEFT JOIN system_currency AS sc ON lm.currency_id = sc.id";
                _query += " WHERE lm.status = 1 and lm.company_id =" + comp.company_id;
                _query += " AND (("+ month +" != 0 AND MONTH(lm.create_date) = "+ month + ") OR ("+ month + " = 0))";
                _query += " AND (("+ year +" != 0 AND YEAR(lm.create_date) = "+ year + ") OR ("+ year + " = 0))) aa";
                _query += " WHERE (('"+ _quo + "' != '' AND aa.bill_no LIKE CONCAT('%',LOWER('"+ _quo + "'),'%')) OR ('"+ _quo + "' = ''))";

                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand(_query, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var payHis = new PayHistory();
                        var _my = "";
                        payHis.Id = Convert.ToInt32(rdr["id"]);
                        payHis.bill_trans = rdr["bill_no"].ToString();
                        payHis.file_code = rdr["file_code"].ToString();
                        payHis.currency_name = rdr["name"].ToString();
                        payHis.rate = Convert.ToDecimal(rdr["rate"]);
                        payHis.amount = Convert.ToDecimal(rdr["amount"]);
                        payHis.amount_thai = Convert.ToDecimal(rdr["amount_thai"]);
                        payHis.create_date = rdr["create_date"].ToString();

                        if (rdr["create_date"].ToString().Substring(3, 1) == "0")
                        {
                            _my = rdr["create_date"].ToString().Substring(6) + rdr["create_date"].ToString().Substring(4, 1);
                        }
                        else
                        {
                            _my = rdr["create_date"].ToString().Substring(6) + rdr["create_date"].ToString().Substring(3, 2);
                        }

                        payHis.my = _my;
                        payHistoryList.Add(payHis);
                    }
                }

                var json = JsonConvert.SerializeObject(Data);

                return Json(new { result = payHistoryList, message = "request successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request failed" });
            }
        }

        public JsonResult SearchMonthYear(int month, int year,int id)
        {
            accountEntities db = new accountEntities();
            var Data = db.bill_transaction.ToList();
            try
            {
                var comp = db.user_member_login.Where(y => y.id == id).FirstOrDefault();
                Data = db.bill_transaction.Where(x => x.month == month && x.year == year && x.company_id == comp.company_id).ToList();

                List<QuotationAll> quotationList = new List<QuotationAll>();
                string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                string query = "SELECT bt.month, bt.year, bt.company_id, c.name, bt.status, SUM(grand_total) grand_total,";
                query += " SUM(btd.balance_amount) balance_amount, SUM(btd.complete_amount) complete_amount, SUM(btd.overpay) overpay";
                query += " FROM bill_transaction bt";
                query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount,";
                query += " SUM(complete_amount) +SUM(overpay) complete_amount, SUM(overpay) overpay";
                query += " FROM bill_transaction_detail";
                query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
                query += " LEFT JOIN company c ON bt.company_id = c.id";
                query += " WHERE bt.month = " + month + " and bt.year = " + year + " and bt.company_id = " + comp.company_id;
                query += " GROUP BY  bt.month, bt.year, bt.status, bt.company_id, c.name";
                using (SqlConnection con = new SqlConnection(CS))
                {
                    SqlCommand cmd = new SqlCommand(query, con);
                    cmd.CommandType = CommandType.Text;
                    con.Open();

                    SqlDataReader rdr = cmd.ExecuteReader();
                    while (rdr.Read())
                    {
                        var quotation = new QuotationAll();
                        
                        quotation.month = Convert.ToInt32(rdr["month"]);
                        quotation.year = Convert.ToInt32(rdr["year"]);
                        quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                        quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                        quotation.name = Convert.ToString(rdr["name"]);
                        quotation.status = Convert.ToInt32(rdr["status"]);
                        quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                        quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                        quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                        quotationList.Add(quotation);
                    }
                }

                var json = JsonConvert.SerializeObject(Data);

                return Json(new { result = quotationList, message = "request successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request failed" });
            }
        }

        public ActionResult UploadSlip(string id,string date)
        {
            accountEntities db = new accountEntities();
            var CurList = db.system_currency.ToList();
            List<ListDropDown> cur = new List<ListDropDown>();
            foreach (var item in CurList.OrderBy(x => x.name))
            {
                ListDropDown iTmp = new ListDropDown();
                iTmp.iKey = item.name;
                iTmp.iValue = item.id.ToString();
                cur.Add(iTmp);
            }
            ViewBag.CurList = cur;

            var year = date.Substring(0, 4);
            var month = date.Substring(4);

            List<QuotationAll> quotationList = new List<QuotationAll>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT bt.id, bt.no, bt.month, bt.year, bt.company_id, bt.status, bt.system_bank_id, bt.grand_total, ";
            query += " btd.balance_amount, btd.complete_amount, btd.overpay ,c.name ,cp.name name_prefix ,bt.start_date,bt.end_date";
            query += " FROM bill_transaction AS bt";
            query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount, SUM(complete_amount) +SUM(overpay) complete_amount,";
            query += " SUM(overpay) overpay FROM bill_transaction_detail";
            query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
            query += " LEFT JOIN company c ON bt.company_id = c.id";
            query += " LEFT JOIN company_prefix cp ON bt.company_prefix_id = cp.id";
            query += " WHERE bt.month = " + month + " AND bt.year = " + year + " AND bt.company_id = " + id;
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var quotation = new QuotationAll();

                    quotation.id = Convert.ToInt32(rdr["id"]);
                    quotation.no = rdr["no"].ToString();
                    quotation.month = Convert.ToInt32(rdr["month"]);
                    quotation.year = Convert.ToInt32(rdr["year"]);
                    quotation.bank = rdr["system_bank_id"].ToString();
                    quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                    quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                    quotation.name = Convert.ToString(rdr["name"]);
                    quotation.status = Convert.ToInt32(rdr["status"]);
                    quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                    quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                    quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                    quotation.start = Convert.ToDateTime(rdr["start_date"]).ToString("dd/MM/yyyy");
                    quotation.end = Convert.ToDateTime(rdr["end_date"]).ToString("dd/MM/yyyy");
                    quotationList.Add(quotation);
                }
            }
            ViewBag.Quotation = quotationList;


            return View();
        }

        public ActionResult EditSlip(int id ,string date,int u)
        {
            accountEntities db = new accountEntities();
            var data = db.log_pay_member.Where(x => x.id == id).FirstOrDefault();
            var comp = db.user_member_login.Where(x => x.id == u).FirstOrDefault();

            var CurList = db.system_currency.ToList();
            List<ListDropDown> cur = new List<ListDropDown>();
            foreach (var item in CurList.OrderBy(x => x.name))
            {
                ListDropDown iTmp = new ListDropDown();
                iTmp.iKey = item.name;
                iTmp.iValue = item.id.ToString();
                cur.Add(iTmp);
            }
            ViewBag.CurList = cur;

            var year = date.Substring(0, 4);
            var month = date.Substring(4);

            List<QuotationAll> quotationList = new List<QuotationAll>();
            string CS = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
            string query = "SELECT bt.id, bt.no, bt.month, bt.year, bt.company_id, bt.status, bt.system_bank_id, bt.grand_total, ";
            query += " btd.balance_amount, btd.complete_amount, btd.overpay ,c.name ,cp.name name_prefix ,bt.start_date,bt.end_date";
            query += " FROM bill_transaction AS bt";
            query += " LEFT JOIN(SELECT bill_transaction_id, SUM(balance_amount) balance_amount, SUM(complete_amount) +SUM(overpay) complete_amount,";
            query += " SUM(overpay) overpay FROM bill_transaction_detail";
            query += " GROUP BY bill_transaction_id) btd ON bt.id = btd.bill_transaction_id";
            query += " LEFT JOIN company c ON bt.company_id = c.id";
            query += " LEFT JOIN company_prefix cp ON bt.company_prefix_id = cp.id";
            query += " WHERE bt.month = " + month + " AND bt.year = " + year + " AND bt.company_id = " + comp.company_id;
            using (SqlConnection con = new SqlConnection(CS))
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                con.Open();

                SqlDataReader rdr = cmd.ExecuteReader();
                while (rdr.Read())
                {
                    var quotation = new QuotationAll();

                    quotation.id = Convert.ToInt32(rdr["id"]);
                    quotation.no = rdr["no"].ToString();
                    quotation.month = Convert.ToInt32(rdr["month"]);
                    quotation.year = Convert.ToInt32(rdr["year"]);
                    quotation.bank = rdr["system_bank_id"].ToString();
                    quotation.grand_total = Convert.ToDecimal(rdr["grand_total"]);
                    quotation.company_id = Convert.ToInt32(rdr["company_id"]);
                    quotation.name = Convert.ToString(rdr["name"]);
                    quotation.status = Convert.ToInt32(rdr["status"]);
                    quotation.complete_amount = Convert.ToDecimal(rdr["complete_amount"]);
                    quotation.overpay = Convert.ToDecimal(rdr["overpay"]);
                    quotation.balance_amount = Convert.ToDecimal(rdr["balance_amount"]);
                    quotation.start = Convert.ToDateTime(rdr["start_date"]).ToString("dd/MM/yyyy");
                    quotation.end = Convert.ToDateTime(rdr["end_date"]).ToString("dd/MM/yyyy");
                    quotationList.Add(quotation);
                }
            }
            ViewBag.Quotation = quotationList;

            var filecode = data.file_code;

            var img = db.file_detail.Where(x => x.file_code == filecode && x.status == true).ToList();
            ViewBag.ImgList = img;

            return View(data);
        }

        //[HttpPost]
        //public static Image LoadBase64(string base64image)
        //{

        //    var t = base64image.Substring(23);  // remove data:image/png;base64,

        //    byte[] bytes = Convert.FromBase64String(t);

        //    Image image;
        //    using (MemoryStream ms = new MemoryStream(bytes))
        //    {
        //        image = Image.FromStream(ms);
        //    }
        //    return image;
        //}

        public static Bitmap LoadBase64(string base64String)
        {
            Bitmap bmpReturn = null;

            var t = base64String.Substring(23);
            byte[] byteBuffer = Convert.FromBase64String(t);
            MemoryStream memoryStream = new MemoryStream(byteBuffer);


            memoryStream.Position = 0;


            bmpReturn = (Bitmap)Bitmap.FromStream(memoryStream);


            memoryStream.Close();
            memoryStream = null;
            byteBuffer = null;


            return bmpReturn;
        }


        [HttpPost]
        public JsonResult insSlip(log_pay infos)//ฟังก์ชั่น insert data to sql
        {
            string msg = "";
            HttpFileCollectionBase _files = Request.Files;

            accountEntities data = new accountEntities();
            var comp = data.user_member_login.Where(x => x.id == Convert.ToInt32(infos.createBy)).FirstOrDefault();
            //Bitmap files = LoadBase64(infos.SlipImg.ToString());
           
            //UploadFile(files, filename);
            //var Model = Request.Form.AllKeys.Length;
            //accountEntities dbcontext = new accountEntities();
            //log_pay_member insSlip = new log_pay_member();
            try
            {
                string fileCode = Guid.NewGuid().ToString();
                if(_files != null)
                {
                    var num = _files.Count;
                    
                    for (int i = 0; i < num; i++)
                    {
                        HttpPostedFileBase _File = _files[i];
                        string g = Guid.NewGuid().ToString();
                        string _type = _files[i].ContentType.ToString();
                        string fname = g + "." + _type.Substring(6);
                        UploadFile1(_File, fname);

                        var newpath = string.Format(WebConfigurationManager.AppSettings["file_pay_url_dev"] + "/SlipImg/" + fname);
                        if (infos != null)
                        {
                            string conn = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                            SqlConnection cnn = new SqlConnection(conn);
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = cnn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "insert_file_detail";
                            //parameter
                            cmd.Parameters.AddWithValue("@pActionID", infos.actionID);
                            cmd.Parameters.AddWithValue("@pActionName", infos.actionName);
                            cmd.Parameters.AddWithValue("@pFileExtension", _type.Substring(6));
                            cmd.Parameters.AddWithValue("@pFileCode", fileCode);
                            cmd.Parameters.AddWithValue("@pName", fname);
                            cmd.Parameters.AddWithValue("@pUrl", newpath);
                            cmd.Parameters.AddWithValue("@pCreateBy", infos.createBy);
                            cnn.Open();
                            object o = cmd.ExecuteScalar();
                            cnn.Close();
                        }
                    }
                }
               

                if (infos != null)
                {
                    string conn = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                    SqlConnection cnn = new SqlConnection(conn);
                    SqlCommand cmd = new SqlCommand();
                    cmd.Connection = cnn;
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.CommandText = "insert_log_pay_member";
                    //parameter
                    cmd.Parameters.AddWithValue("@pBilltran", infos.billtrans);
                    cmd.Parameters.AddWithValue("@pCurrencyId", infos.currencyId);
                    cmd.Parameters.AddWithValue("@pRate", infos.rate);
                    cmd.Parameters.AddWithValue ("@pAmount", infos.amount);
                    cmd.Parameters.AddWithValue("@pFileCode", fileCode);
                    cmd.Parameters.AddWithValue("@pAccountType", infos.accountType);
                    cmd.Parameters.AddWithValue("@pCompID", comp.company_id);
                    cmd.Parameters.AddWithValue("@pCreateBy", infos.createBy);
                    cnn.Open();
                    object o = cmd.ExecuteScalar();
                    cnn.Close();
                }

                msg = "Upload slip complete!";
                
            }
            catch (DbEntityValidationException e)
            {
                foreach (var eve in e.EntityValidationErrors)
                {
                    Console.WriteLine("Entity of type \"{0}\" in state \"{1}\" has the following validation errors:",
                        eve.Entry.Entity.GetType().Name, eve.Entry.State);
                    foreach (var ve in eve.ValidationErrors)
                    {
                        Console.WriteLine("- Property: \"{0}\", Error: \"{1}\"",
                            ve.PropertyName, ve.ErrorMessage);
                    }
                }
                msg = "error !!";
            }
            return Json(new { result = msg, message = "request successfully" });
        }

        [HttpPost]
        public ActionResult UploadFile1(HttpPostedFileBase file,string fname)
        {
            if (file != null && file.ContentLength > 0)
                try
                {  //Server.MapPath takes the absolte path of folder 'Uploads'
                    //string guid = Guid.NewGuid().ToString();
                    //string _type = file.ContentType.ToString();
                    //string fname = guid + "."+ _type.Substring(6);
                    string path = Path.Combine(Server.MapPath("~/SlipImg"),
                                               Path.GetFileName(fname));
                    //Save file using Path+fileName take from above string
                    file.SaveAs(path);
                    ViewBag.Message = "File uploaded successfully";
                }
                catch (Exception ex)
                {
                    ViewBag.Message = "ERROR:" + ex.Message.ToString();
                }
            else
            {
                ViewBag.Message = "You have not specified a file.";
            }
            return View();
        }

        //[HttpPost]
        //public ActionResult UploadFile(Bitmap bmp,string filename)
        //{
        //    string path = Path.Combine(Server.MapPath("~/SlipImg"), filename);
        //    Bitmap bitmap = new Bitmap(bmp.Width, bmp.Height, bmp.PixelFormat);
        //    Graphics g = Graphics.FromImage(bitmap);
        //    g.DrawImage(bmp, new Point(0, 0));
        //    g.Dispose();
        //    bmp.Dispose();
        //    bitmap.Save(path);
        //    bmp = bitmap;
        //    //string path = Path.Combine(Server.MapPath("~/SlipImg"), filename);
        //    //string outputFileName = filename;
        //    //using (MemoryStream memory = new MemoryStream())
        //    //{
        //    //    using (FileStream fs = new FileStream(path, FileMode.Create, FileAccess.ReadWrite))
        //    //    {
        //    //        file.Save(memory, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    //        byte[] bytes = memory.ToArray();
        //    //        fs.Write(bytes, 0, bytes.Length);
        //    //    }
        //    //}
        //    //if (file != null /*&& file.ContentLength > 0*/)
        //    //    try
        //    //    {  //Server.MapPath takes the absolte path of folder 'Uploads'
        //    //        string path = Path.Combine(Server.MapPath("~/SlipImg"), filename);
        //    //        //Save file using Path+fileName take from above string
        //    //        file.Save(path, System.Drawing.Imaging.ImageFormat.Jpeg);
        //    //        file.Dispose();
        //    //        ViewBag.Message = "File uploaded successfully";
        //    //    }
        //    //    catch (Exception ex)
        //    //    {
        //    //        ViewBag.Message = "ERROR:" + ex.Message.ToString();
        //    //    }
        //    //else
        //    //{
        //    //    ViewBag.Message = "You have not specified a file.";
        //    //}
        //    return View();
        //}

        [HttpGet]
        public ActionResult ShowImg(int id)
        {
            accountEntities db = new accountEntities();
            var model = db.log_pay_member.Where(m => m.id == id).FirstOrDefault();
            //return PartialView(@"Views/Shared/_imgshow.cshtml", model);
            return PartialView("imgshow", model);
        }

        [HttpGet]
        public ActionResult imgshow()
        {

            return PartialView();
        }

        public JsonResult UpdateBillTrans(bill_transaction data) // update data ลง sql
        {
            try
            {
                var db = new accountEntities();
                var Data = db.bill_transaction.Where(x => x.id == data.id).FirstOrDefault();

                if (Data != null)
                {
                    Data.is_complete = 2;
                    Data.complete_date = DateTime.Now;
                    Data.complete_by = data.complete_by;
                    var i = db.SaveChanges();

                }
                else
                {
                    return Json(new { result = "no data !!", message = "request successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request successfully" });
            }

            return Json(new { result = "Update Car Complete!", message = "request successfully" });
        }

        [HttpPost]
        public JsonResult DelImg(int id, int img) // update data ลง sql
        {
            try
            {
                
                var db = new accountEntities();
                var Data = db.file_detail.Where(x => x.id == img).FirstOrDefault();

                if (Data != null)
                {
                   
                    Data.status = false;
                    Data.update_by = id;
                    Data.update_date = DateTime.Now;
                    
                    var i = db.SaveChanges();

                }
                else
                {
                    return Json(new { result = "no data !!", message = "request successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request successfully" });
            }

            return Json(new { result = "Complete!", message = "request successfully" });
        }

        public JsonResult UpdateSlip(log_pay data) // update data ลง sql
        {
            try
            {
                HttpFileCollectionBase _files = Request.Files;
                accountEntities db = new accountEntities();
                var _id = db.log_pay_member.Where(x => x.id == data.Id).FirstOrDefault();

                if (_files != null)
                {
                    var num = _files.Count;

                    for (int i = 0; i < num; i++)
                    {
                        HttpPostedFileBase _File = _files[i];
                        string g = Guid.NewGuid().ToString();
                        string _type = _files[i].ContentType.ToString();
                        string fname = g + "." + _type.Substring(6);
                        UploadFile1(_File, fname);
                        var newpath = string.Format(WebConfigurationManager.AppSettings["file_pay_url_dev"] + "/SlipImg/" + fname);
                        if (data != null)
                        {
                            string conn = ConfigurationManager.ConnectionStrings["DEV"].ConnectionString;
                            SqlConnection cnn = new SqlConnection(conn);
                            SqlCommand cmd = new SqlCommand();
                            cmd.Connection = cnn;
                            cmd.CommandType = CommandType.StoredProcedure;
                            cmd.CommandText = "insert_file_detail";
                            //parameter
                            cmd.Parameters.AddWithValue("@pActionID", data.actionID);
                            cmd.Parameters.AddWithValue("@pActionName", data.actionName);
                            cmd.Parameters.AddWithValue("@pFileExtension", _type.Substring(6));
                            cmd.Parameters.AddWithValue("@pFileCode", _id.file_code);
                            cmd.Parameters.AddWithValue("@pName", fname);
                            cmd.Parameters.AddWithValue("@pUrl", newpath);
                            cmd.Parameters.AddWithValue("@pCreateBy", data.createBy);
                            cnn.Open();
                            object o = cmd.ExecuteScalar();
                            cnn.Close();
                        }
                    }
                }

                      
                var Data = db.log_pay_member.Where(x => x.id == data.Id).FirstOrDefault();

                if (Data != null)
                {
                    Data.account_type = data.accountType;
                    Data.amount = data.amount;
                    Data.currency_id = data.currencyId;
                    Data.rate = data.rate;
                    Data.amount_thai = data.amount * data.rate;
                    var i = db.SaveChanges();

                }
                else
                {
                    return Json(new { result = "no data !!", message = "request successfully" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request successfully" });
            }

            return Json(new { result = "Update Car Complete!", message = "request successfully" });
        }
    }

}
