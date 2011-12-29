@echo off

set fdir=%WINDIR%\Microsoft.NET\Framework64

if not exist %fdir% (
	set fdir=%WINDIR%\Microsoft.NET\Framework
)

set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocketClient\WebSocketClient\WebSocketClient.csproj /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\..\superwebsocket.snk /p:OutputPath=..\..\bin\Net40
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

%msbuild% WebSocketClient\WebSocketClient\WebSocketClient.Mono.csproj /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\..\superwebsocket.snk /p:OutputPath=..\..\bin\Mono
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

set fdir=%WINDIR%\Microsoft.NET\Framework
set msbuild=%fdir%\v4.0.30319\msbuild.exe

%msbuild% WebSocketClient\WebSocketClient.Silverlight\WebSocketClient.Silverlight.csproj  /p:Configuration=Release /t:Rebuild /p:SignAssembly=true /p:AssemblyOriginatorKeyFile=..\..\superwebsocket.snk /p:OutputPath=..\..\bin\Silverlight
FOR /F "tokens=*" %%G IN ('DIR /B /AD /S obj') DO RMDIR /S /Q "%%G"

pause