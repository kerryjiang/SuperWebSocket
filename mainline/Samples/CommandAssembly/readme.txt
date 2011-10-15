If you use BasiSubProtocol directly, you only need to create your own command assembly project like this one.

How to use this CommandAssembly?

1. Copy this project's output to working directory of SuperWebSocket's running container,
SuperWebSocketService, SuperWebSocketWeb or your own container.

2. Add sub protocol command assembly definition to the websocket server instance's configuration element of SuperWebSocket's running container,
like the file SuperWebSocket.Service.exe.config in current project. 
<subProtocols>
	<protocol>
		<commands>
			<add assembly="SuperWebSocket.Samples.CommandAssembly"/>
		</commands>
	</protocol>
</subProtocols>

3. SuperWebSocket also support multiple command assemblies, you can define many command nodes sub protocol element.

<subProtocols>
	<protocol>
		<commands>
			<add assembly="SuperWebSocket.Samples.CommandAssemblyA"/>
			<add assembly="SuperWebSocket.Samples.CommandAssemblyB"/>
		</commands>
	</protocol>
</subProtocols>