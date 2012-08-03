using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Configuration;

namespace SuperWebSocketWeb
{
    public partial class LiveChatWithBridge : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            var nameCookie = Request.Cookies.Get("name");

            if (nameCookie == null)
                Response.Redirect("~/Default.aspx?returnUrl=" + Server.UrlEncode(Request.FilePath));
        }

        protected object WebSocketPort
        {
            get
            {
                var extPort = ConfigurationManager.AppSettings["extPort"];

                if (string.IsNullOrEmpty(extPort))
                    return Application["WebSocketPort"];
                else
                    return extPort;
            }
        }
    }
}