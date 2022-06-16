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

namespace account.Controllers
{
    public class QuotationController : Controller
    {
        // GET: Quotation
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Menu()
        {
            return View();
        }

        public ActionResult SiteMaster()
        {
            return View();
        }

        public RedirectResult Quotations(int Id)
        {
            //Treatment treatment = new Treatment();
            ////ทดลองอ่าน json flie รอ crystal report
            //using (StreamReader sr = new StreamReader(Server.MapPath("~/Content/data.json")))
            //{
            //    ViewBag.quotation = JsonConvert.DeserializeObject<Treatment>(sr.ReadToEnd());
            //}
            return Redirect("/viewQuotation.aspx?id=" + Id);

        }

        public RedirectResult QuotationsPaid(int Id)
        {
            //Treatment treatment = new Treatment();
            ////ทดลองอ่าน json flie รอ crystal report
            //using (StreamReader sr = new StreamReader(Server.MapPath("~/Content/data.json")))
            //{
            //    ViewBag.quotation = JsonConvert.DeserializeObject<Treatment>(sr.ReadToEnd());
            //}
            return Redirect("/viewQuotationPaid.aspx?id=" + Id);

        }

        public ActionResult ListQuotation(int id)
        {
            int iMonth = 0, iYear = 0;
            iMonth = DateTime.Now.Month;
            iYear = DateTime.Now.Year;
            //list from create because comnyid in system_user = 1 all
            var db = new accountEntities();
            var _comp = db.user_login.Where(x => x.id == id).FirstOrDefault();
            var aaa = db.bill_transaction.Where(z => z.month == iMonth && z.year == iYear && z.create_by == id).ToList();

            return View(aaa);
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

        public JsonResult SearchQuotationDate(string start_date, string end_date)
        {
            try
            {
                var db = new accountEntities();
                DateTime sd = DateTime.ParseExact(start_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                DateTime ed = DateTime.ParseExact(end_date, "yyyy-MM-dd", CultureInfo.InvariantCulture);
                var Data = db.bill_transaction.Where(x => x.start_date >= sd && x.end_date <= ed).ToList();

                var json = JsonConvert.SerializeObject(Data);

                return Json(new { result = json, message = "request successfully" });
            }
            catch (Exception ex)
            {
                return Json(new { result = "error", message = "request failed" });
            }
        }

        public ActionResult UploadSlip(int id)
        {
            accountEntities db = new accountEntities();
            var list = db.bill_transaction.Where(x => x.id == id).FirstOrDefault();

            return View(list);
        }

        [HttpPost]
        public JsonResult insSlip(bill_confirm_slip infos)//ฟังก์ชั่น insert data to sql
        {
            string msg = "";
            HttpFileCollectionBase files = Request.Files;
            var Model = Request.Form.AllKeys.Length;
            accountEntities dbcontext = new accountEntities();
            bill_confirm_slip insSlip = new bill_confirm_slip();
            try
            {
                if (infos != null)
                {
                    insSlip.transaction_id = Convert.ToInt32(Request.Form["transaction_id"].ToString().Trim());
                    insSlip.create_date = DateTime.Now;
                    insSlip.amount = Convert.ToInt32(Request.Form["amount"].ToString().Trim());
                    

                    if (files.Count > 0)
                    {
                        HttpPostedFileBase _File = files[0];
                        insSlip.slip = _File.FileName;
                        UploadFile(_File);
                    }
                    else
                    {
                        insSlip.slip = "";
                    }

                    dbcontext.bill_confirm_slip.Add(insSlip);
                    var i = dbcontext.SaveChanges();
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
        public ActionResult UploadFile(HttpPostedFileBase file)
        {
            if (file != null && file.ContentLength > 0)
                try
                {  //Server.MapPath takes the absolte path of folder 'Uploads'
                    string path = Path.Combine(Server.MapPath("~/SlipImg"),
                                               Path.GetFileName(file.FileName));
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
        
    }
}