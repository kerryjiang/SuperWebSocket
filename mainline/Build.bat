@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% SuperWebSocket\SuperWebSocket.csproj /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\superwebsocket.snk /p:OutputPath=..\bin\Net40

%msbuild% SuperWebSocket\SuperWebSocket.NET35.csproj  /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\superwebsocket.snk /p:OutputPath=..\bin\Net35

pause