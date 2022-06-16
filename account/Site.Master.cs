using account.core;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using Microsoft.AspNet.Identity;
using System.Text;
using System.Data;

namespace account
{
    public partial class SiteMaster : MasterPage
    {
        SqlManager _sql = new SqlManager();
        private const string AntiXsrfTokenKey = "__AntiXsrfToken";
        private const string AntiXsrfUserNameKey = "__AntiXsrfUserName";
        private string _antiXsrfTokenValue;

        protected void Page_Init(object sender, EventArgs e)
        {
            // The code below helps to protect against XSRF attacks
            var requestCookie = Request.Cookies[AntiXsrfTokenKey];
            Guid requestCookieGuidValue;
            if (requestCookie != null && Guid.TryParse(requestCookie.Value, out requestCookieGuidValue))
            {
                // Use the Anti-XSRF token from the cookie
                _antiXsrfTokenValue = requestCookie.Value;
                Page.ViewStateUserKey = _antiXsrfTokenValue;
            }
            else
            {
                // Generate a new Anti-XSRF token and save to the cookie
                _antiXsrfTokenValue = Guid.NewGuid().ToString("N");
                Page.ViewStateUserKey = _antiXsrfTokenValue;

                var responseCookie = new HttpCookie(AntiXsrfTokenKey)
                {
                    HttpOnly = true,
                    Value = _antiXsrfTokenValue
                };
                if (FormsAuthentication.RequireSSL && Request.IsSecureConnection)
                {
                    responseCookie.Secure = true;
                }
                Response.Cookies.Set(responseCookie);
            }

            Page.PreLoad += master_Page_PreLoad;
        }

        protected void master_Page_PreLoad(object sender, EventArgs e)
        {
            if (!IsPostBack)
            {
                // Set Anti-XSRF token
                ViewState[AntiXsrfTokenKey] = Page.ViewStateUserKey;
                ViewState[AntiXsrfUserNameKey] = Context.User.Identity.Name ?? String.Empty;
            }
            else
            {
                // Validate the Anti-XSRF token
                if ((string)ViewState[AntiXsrfTokenKey] != _antiXsrfTokenValue
                    || (string)ViewState[AntiXsrfUserNameKey] != (Context.User.Identity.Name ?? String.Empty))
                {
                    throw new InvalidOperationException("Validation of Anti-XSRF token failed.");
                }
            }
        }

        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.Cookies["Keys"] == null)
            {
                Response.Redirect("../Login.aspx");
            }
            else if (Request.Cookies["Keys"] != null)
            {
                HttpCookieCollection cookies = Request.Cookies;
                for (int i = 0; i < cookies.Count; i++)
                {
                    var All = cookies[i].Value;

                    if (cookies[i].Name == "Keys")
                    {
                        ID_user.Value = All.Split('&')[0].Split('=')[1];
                        LoginUser.Value = All.Split('&')[1].Split('=')[1];
                        LoginPosi.Value = All.Split('&')[3].Split('=')[1];
                        LoginPosi_ID.Value = All.Split('&')[4].Split('=')[1];
                        Token_ID.Value = All.Split('&')[5].Split('=')[1];
                    }
                    else if (cookies[i].Name == "lan")
                    {
                        Language.Value = All;
                    }
                }
            }

            int Check_token = _sql.Check_Token_Id(Token_ID.Value);
            if (Check_token == 0)
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "setTimeout(function(){ LogOut(); });", true);
            }
        }

        protected void Unnamed_LoggingOut(object sender, LoginCancelEventArgs e)
        {
            Context.GetOwinContext().Authentication.SignOut(DefaultAuthenticationTypes.ApplicationCookie);
        }

        public void Logout_click(Object sender, EventArgs e)
        {
            DataTable table = _sql.Logout(Int32.Parse(ID_user.Value));

            if (table.Rows.Count > 0)
            {
                if (Request.Cookies["Keys"] != null)
                {
                    Response.Cookies["Keys"].Expires = DateTime.Now.AddDays(-1);
                }

                if (Request.Cookies["lan"] != null)
                {
                    Response.Cookies["lan"].Expires = DateTime.Now.AddDays(-1);
                }

                Response.Redirect("../Login.aspx");
            }
            else
            {
                Page.ClientScript.RegisterStartupScript(GetType(), "msgbox", "alert('Error : Log out.');", true);
            }
        }

    }

}