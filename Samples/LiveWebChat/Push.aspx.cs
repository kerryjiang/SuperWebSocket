using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Threading;

namespace SuperWebSocketWeb
{
    public partial class Push : System.Web.UI.Page
    {
        protected void Page_Load(object sender, EventArgs e)
        {
            if (Request.QueryString["pooling"] != null)
            {
                Response.Clear();
                Response.BufferOutput = false;
                Pooling();
            }
        }

        private void Pooling()
        {
            int round = 1000;

            while(round > 0)
            {
                Thread.Sleep(60000);
                Response.Write(string.Format("Time: {0}", DateTime.Now));
                Response.Flush();
                round--;
            }
        }
    }
}