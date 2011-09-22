<%@ Page Language="C#" AutoEventWireup="true" CodeBehind="LiveChat.aspx.cs" Inherits="SuperWebSocketWeb.LiveChat" %>
<!DOCTYPE html PUBLIC "-//W3C//DTD XHTML 1.0 Transitional//EN" "http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd">
<html xmlns="http://www.w3.org/1999/xhtml">
<head runat="server">
    <title>Live Chat</title>
    <script type="text/javascript" src="Scripts/jquery.js"></script>
    <style type="text/css">
    body
    {
        margin:0px;
        padding:0px;
        font-size:12px;
    }
    .panelHeader
    {
        color:#15428b;
        font-weight:bold; 
        font-size: 11px;
        font-family: tahoma,arial,verdana,sans-serif;
        border-color:#99bbe8;
        background-image: url(Images/white-top-bottom.gif);
        line-height:26px;
        padding-left:10px;
    }
    #messageBoard
    {
        overflow: scroll;
        padding-bottom:100px;
    }
    </style>
    <script type="text/javascript">
        var noSupportMessage = "Your browser cannot support WebSocket!";
        var ws;

        function resizeFrame() {
            var h = $(window).height();
            var w = $(window).width();
            //Adapt screen height
            $('#messageBoard').css("height", (h - 80 - 50 - 100) + "px");
            $('#messageBoxCell').css("width", (w - 100) + "px");
            $('#messageBox').css("width", (w - 110) + "px");
        }

        $(document).keypress(function (e) {
            if (e.ctrlKey && e.which == 13 || e.which == 10) {
                $("#btnSend").click();
                document.body.focus();
            } else if (e.shiftKey && e.which == 13 || e.which == 10) {
                $("#btnSend").click();
                document.body.focus();
            }
        })

        function scrollToBottom(target) {
            target.animate({ scrollTop: target[0].scrollHeight });
        }

        function connectSocketServer() {
            var messageBoard = $('#messageBoard');

            var support = "MozWebSocket" in window ? 'MozWebSocket' : ("WebSocket" in window ? 'WebSocket' : null);

            if (support == null) {
                alert(noSupportMessage);
                messageBoard.append("* " + noSupportMessage + "<br/>");
                return;
            }

            messageBoard.append("* Connecting to server ..<br/>");
            // create a new websocket and connect
            ws = new window[support]('ws://<%= Request.Url.Host %>:<%= WebSocketPort %>/sample');

            // when data is comming from the server, this metod is called
            ws.onmessage = function (evt) {
                messageBoard.append("# " + evt.data + "<br />");
                scrollToBottom(messageBoard);
            };

            // when the connection is established, this method is called
            ws.onopen = function () {
                messageBoard.append('* Connection open<br/>');
            };

            // when the connection is closed, this method is called
            ws.onclose = function () {
                messageBoard.append('* Connection closed<br/>');
            }

            //setup secure websocket
            var wss = new window[support]('wss://<%= Request.Url.Host %>:<%= SecureWebSocketPort %>/sample');

            // when data is comming from the server, this metod is called
            wss.onmessage = function (evt) {
                messageBoard.append("# " + evt.data + "<br />");
                scrollToBottom(messageBoard);
            };

            // when the connection is established, this method is called
            wss.onopen = function () {
                messageBoard.append('* Secure Connection open<br/>');
            };

            // when the connection is closed, this method is called
            wss.onclose = function () {
                messageBoard.append('* Secure Connection closed<br/>');
            }
        }

        function sendMessage() {
            if (ws) {
                var messageBox = document.getElementById('messageBox');
                ws.send(messageBox.value);
                messageBox.value = "";
            } else {
                alert(noSupportMessage);
            }
        }

        jQuery.event.add(window, "resize", resizeFrame);

        window.onload = function () {
            resizeFrame();
            connectSocketServer();
        }
    
    </script>
</head>
<body>
    <form id="formMain" runat="server">
        <table width="100%" cellspacing="1" bgcolor="#99BBE8" cellpadding="0" border="0">
            <tr>
                <td class="panelHeader">Chat Message</td>
            </tr>
            <tr>
                <td bgcolor="#ffffff">
                    <div id="messageBoard"></div>                
                </td>
            </tr>
            <tr>
                <td bgcolor="#D2E0F0">
                    <table width="100%" border="0" height="90" cellpadding="2" cellspacing="1">
                        <tr>
                            <td id="messageBoxCell">
                                <textarea id="messageBox" style="height:80px; border: 1px solid gray;"></textarea>
                            </td>
                            <td width="100" valign="top" align="center">
                                <input type="button" id="btnSend"
                                        value="Send"
                                        style="width:84px; height:64px;"
                                        onclick="return sendMessage();" />
                                <span style="font-size:12px;color:Red;">[Ctrl + Enter]</span>
                            </td>
                        </tr>
                    </table>
                </td>
            </tr>
        </table>
    </form>
</body>
</html>
