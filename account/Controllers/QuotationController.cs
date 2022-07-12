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

            //Default It will Select Current Year  
            return new SelectList(ddlYears.OrderByDescending(x=>x.Value), "Value", "Text", iSelectedYear);

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

        public ActionResult HistoryList(int id, int? page)
        {
            accountEntities db = new accountEntities();
            var data = db.bill_transaction.Where(x => x.id == id /*&& x.is_complete == 3*/).ToList();

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(data.ToPagedList(pageNumber, pageSize));
        }

        //public ActionResult History(int id, int? page)
        //{
        //    accountEntities db = new accountEntities();
        //    var data = db.bill_confirm_slip.Where(x => x.transaction_id == id).ToList();

        //    int pageSize = 20;
        //    int pageNumber = (page ?? 1);
        //    return View(data.ToPagedList(pageNumber, pageSize));
        //}

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
            var _comp = db.user_login.Where(x => x.id == id).FirstOrDefault();

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

            //var balance = db.bill_transaction_detail.Where(x => x.bill_transaction_id == id).ToList();
            //ViewBag.Balance = balance;

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(quotationList.ToPagedList(pageNumber, pageSize));
            //return View(aaa);
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


            //accountEntities db = new accountEntities();
            //var data = db.bill_confirm_slip.Where(x => x.transaction_id == id).ToList();

            int pageSize = 20;
            int pageNumber = (page ?? 1);
            return View(quotationList.ToPagedList(pageNumber, pageSize));
        }

        public JsonResult getNameCompany(int id)
        {
            var db = new accountEntities();
            var _user = db.user_login.Where(x => x.id == id).FirstOrDefault();
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

                    //quotation.id = Convert.ToInt32(rdr["id"]);
                    //quotation.no = rdr["no"].ToString();
                    bank.bank = rdr["bank_name"].ToString();
                    bank.type = Convert.ToInt32(rdr["bank_type"]);
                    bank.account = rdr["account_no"].ToString();
                    bank.accountName = rdr["account_name"].ToString();
                   
                    bankList.Add(bank);
                }
            }
            //var db = new accountEntities();
            //var _user = db.user_login.Where(x => x.id == id).FirstOrDefault();
            //var comp = db.companies.Where(y => y.id == _user.company_id).FirstOrDefault();
            //var compID = comp.name;

            //ViewBag.company = compID;
            return Json(new { result = bankList }, JsonRequestBehavior.AllowGet);
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
                var comp = db.user_login.Where(y => y.id == id).FirstOrDefault();
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

        public JsonResult SearchMonthYear(int month, int year,int id)
        {
            accountEntities db = new accountEntities();
            var Data = db.bill_transaction.ToList();
            try
            {
                var comp = db.user_login.Where(y => y.id == id).FirstOrDefault();
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

                        //quotation.id = Convert.ToInt32(rdr["id"]);
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
                        //quotation.start = Convert.ToDateTime(rdr["start_date"]).ToString("dd/MM/yyyy");
                        //quotation.name_prefix = rdr["name_prefix"].ToString();
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

        [HttpPost]
        public static Image LoadBase64(string base64image)
        {
           
            var t = base64image.Substring(23);  // remove data:image/png;base64,

            byte[] bytes = Convert.FromBase64String(t);

            Image image;
            using (MemoryStream ms = new MemoryStream(bytes))
            {
                image = Image.FromStream(ms);
            }
            return image;
        }


        [HttpPost]
        public JsonResult insSlip(log_pay infos)//ฟังก์ชั่น insert data to sql
        {
            string msg = "";

            System.Drawing.Image files = LoadBase64(infos.SlipImg.ToString());
            string filename = Guid.NewGuid().ToString()+"."+infos.fileExtension;

            UploadFile(files, filename);
            //var Model = Request.Form.AllKeys.Length;
            //accountEntities dbcontext = new accountEntities();
            //log_pay_member insSlip = new log_pay_member();
            try
            {
                //Guid guid = Guid.NewGuid();
                
                var newpath = string.Format(WebConfigurationManager.AppSettings["file_pay_url_dev"] + "/SlipImg/"+ filename);
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
                    cmd.Parameters.AddWithValue("@pFileExtension", infos.fileExtension);
                    cmd.Parameters.AddWithValue("@pFileCode", infos.fileCode);
                    cmd.Parameters.AddWithValue("@pName", filename);
                    cmd.Parameters.AddWithValue("@pUrl", newpath);
                    cmd.Parameters.AddWithValue("@pCreateBy", infos.createBy);
                    cnn.Open();
                    object o = cmd.ExecuteScalar();
                    cnn.Close();
                }

                msg = "Upload slip complete!";




                //if (infos != null)
                //{
                //    //insSlip.transaction_id = Convert.ToInt32(Request.Form["transaction_id"].ToString().Trim());
                //    //insSlip.create_date = DateTime.Now;
                //    //insSlip.amount = Convert.ToInt32(Request.Form["amount"].ToString().Trim());


                //    if (files.Count > 0)
                //    {
                //        //HttpPostedFileBase _File = files[0];
                //        //insSlip.slip = _File.FileName;
                //        //UploadFile(_File);
                //    }
                //    else
                //    {
                //        //insSlip.slip = "";
                //    }

                //    dbcontext.log_pay_member.Add(insSlip);
                //    var i = dbcontext.SaveChanges();
                //}
                //msg = "Upload slip complete!";

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
        public ActionResult UploadFile(System.Drawing.Image file,string filename)
        {
            if (file != null /*&& file.ContentLength > 0*/)
                try
                {  //Server.MapPath takes the absolte path of folder 'Uploads'
                    string path = Path.Combine(Server.MapPath("~/SlipImg"), filename
                                               /*Path.GetFileName(file.FileName)*/);
                    //Save file using Path+fileName take from above string
                    file.Save(path);
                    file.Dispose();
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

    }
}