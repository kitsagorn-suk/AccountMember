using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace account.Models
{
    public class Pagination<T>
    {
        public int total { get; set; } = 0;
        public int per_page { get; set; } = 0;
        public int current_page { get; set; } = 0;
        public int last_page { get; set; } = 0;
        public int next_page { get; set; } = 0;
        public int prev_page { get; set; } = 0;
        public int from { get; set; } = 0;
        public int to { get; set; } = 0;
        public List<T> data = new List<T>();
        //public List<NewMenuList> menuList { get; set; }

        public void SetPagination(int total, int perPage, int current)
        {
            this.total = total;
            this.per_page = perPage;
            this.current_page = current;

            double lastpage = total / Convert.ToDouble(per_page);
            this.last_page = Convert.ToInt32(Math.Ceiling(lastpage));
            this.from = (per_page * current) - (per_page - 1);
            this.to = (total < perPage) ? total : per_page * current;
            this.next_page = (current < this.last_page) ? current + 1 : 0;
            this.prev_page = (current > 1) ? current - 1 : 0;

            if (total == 0)
            {
                this.from = 0;
                this.to = 0;
            }
            if (current == this.last_page)
            {
                this.to = total;
            }
        }

        private string GetNextUrl()
        {
            return GetUrl(1);
        }

        private string GetPrevUrl()
        {
            return GetUrl(-1);
        }

        private string GetUrl(int addPage)
        {
            var uri = HttpContext.Current.Request.Url.AbsoluteUri;
            //NameValueCollection parameters = null;
            //if (!uri.Contains("?"))
            //{
            //    var str = "?page=" + current_page + "&limit=" + per_page;
            //    parameters = HttpUtility.ParseQueryString(str);

            //}
            //else
            //{
            //    var queryString = uri.Substring(uri.IndexOf('?'));
            //    parameters = HttpUtility.ParseQueryString(queryString);
            //    if (parameters == null)
            //    {
            //        var str = "?page=" + current_page + "&limit=" + per_page;
            //        parameters = HttpUtility.ParseQueryString(str);
            //    }
            //}


            //parameters["page"] = "" + (current_page + addPage);

            //return uri.Split('?')[0] + "?" + parameters.ToString();
            return uri;

        }
    }
}