using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SuperWebSocketWeb
{
    public partial class LiveChat : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var nameCookie = Request.Cookies.Get("name");

            if (nameCookie == null)
                Response.Redirect("~/Default.aspx");
        }
    }
}