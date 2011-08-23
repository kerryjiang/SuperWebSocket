If you use BasiSubProtocol directly, you only need to create your own command assembly project like this one.

How to use this JsonCommandAssembly?

1. Copy this project's output to working directory of SuperWebSocket's running container,
SuperWebSocketService, SuperWebSocketWeb or your own container.

2. Add attrubute commandAssembly="SuperWebSocket.Samples.JsonCommandAssembly" to the websocket server instance's configuration element of SuperWebSocket's running container,
like the file SuperWebSocket.Service.exe.config in current project.