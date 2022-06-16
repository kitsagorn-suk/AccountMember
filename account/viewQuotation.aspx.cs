using CrystalDecisions.CrystalReports.Engine;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using ConnectionInfo = CrystalDecisions.Shared.ConnectionInfo;

namespace account
{
    public partial class test : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            ReportDocument rpt1 = new ReportDocument();

            rpt1.Load(Server.MapPath("\\Report\\rptQuotation.rpt"));

            rpt1.SetParameterValue("billtranid", Page.ClientQueryString.Substring(3));

            rpt1.SetDatabaseLogon("sa", "Snocko2020");

            CrystalReportViewer1.ReportSource = rpt1;
        }
    }
}