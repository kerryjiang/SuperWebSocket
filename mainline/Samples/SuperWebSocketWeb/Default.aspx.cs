using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;

namespace SuperWebSocketWeb
{
    public partial class Default : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {

        }

        protected void btnLogin_Click(object sender, EventArgs e)
        {
            Response.AppendCookie(new HttpCookie("name", txbName.Text.Trim()));
            var returnUrl = Request.QueryString["returnUrl"];
            Response.Redirect(string.IsNullOrEmpty(returnUrl) ? "~/LiveChatWithBridge.aspx" : returnUrl);
        }
    }
}