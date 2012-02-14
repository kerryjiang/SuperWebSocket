var app;
var silverlightHostID = 'silverlightControlHost';
var silverlightControlID = 'silverlightControl';
var silverlightControlLoaded = false;

function createWebSocket(uri, protocol, onopen, onclose, onmessage, oncreated) {
    var support = "MozWebSocket" in window ? 'MozWebSocket' : ("WebSocket" in window ? 'WebSocket' : null);

    if (support != null) {
        var websocket;
        if (protocol && protocol.length > 0)
            websocket = new window[support](uri, protocol);
        else
            websocket = new window[support](uri);
        websocket.onopen = onopen;
        websocket.onclose = onclose;
        websocket.onmessage = onmessage;
        oncreated(websocket);
        return;
    }

    if (Silverlight && Silverlight.isInstalled()) {
        if (app == null || app == undefined) {
            this.onSilverlightLoaded = function (sender, args) {
                silverlightControlLoaded = true;
                createBirdgeWebSocket(uri, protocol, onopen, onclose, onmessage, oncreated);
            }
            app = createBridgeApp();
            return;
        }

        createBirdgeWebSocket(uri, protocol, onopen, onclose, onmessage, oncreated)
        return;
    }

    alert("Your browser cannot support WebSocket!");
}

function createBirdgeWebSocket(uri, protocol, onopen, onclose, onmessage, oncreated) {
    var slPlugin = document.getElementById('silverlightControl');
    if (slPlugin) {
        var websocket = slPlugin.content.services.createObject("WebSocket");
        if (websocket) {
            websocket.onopen = function (s, e) {
                onopen();
            };
            websocket.onclose = function (s, e) {
                onclose();
            };
            websocket.onmessage = function (s, e) {
                onmessage(e);
            };

            websocket.open(uri, protocol);
            oncreated(websocket);
        }
    }
}

function createBridgeApp() {
    return Silverlight.createObject("ClientBin/WebSocket4Net.JsBridge.xap",
                document.getElementById(silverlightHostID),
                silverlightControlID,
                {
                    width: "0",
                    height: "0",
                    background: "white",
                    version: "4.0.60310.0",
                    autoUpgrade: true
                },
                {
                    onError: onSilverlightError,
                    onLoad: onSilverlightLoaded
                });
}

function onSilverlightError(sender, args) {
    var appSource = "";
    if (sender != null && sender != 0) {
        appSource = sender.getHost().Source;
    }

    var errorType = args.ErrorType;
    var iErrorCode = args.ErrorCode;

    if (errorType == "ImageError" || errorType == "MediaError") {
        return;
    }

    var errMsg = "Unhandled Error in Silverlight Application " + appSource + "\n";

    errMsg += "Code: " + iErrorCode + "    \n";
    errMsg += "Category: " + errorType + "       \n";
    errMsg += "Message: " + args.ErrorMessage + "     \n";

    if (errorType == "ParserError") {
        errMsg += "File: " + args.xamlFile + "     \n";
        errMsg += "Line: " + args.lineNumber + "     \n";
        errMsg += "Position: " + args.charPosition + "     \n";
    }
    else if (errorType == "RuntimeError") {
        if (args.lineNumber != 0) {
            errMsg += "Line: " + args.lineNumber + "     \n";
            errMsg += "Position: " + args.charPosition + "     \n";
        }
        errMsg += "MethodName: " + args.methodName + "     \n";
    }

    throw new Error(errMsg);
}