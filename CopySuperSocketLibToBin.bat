@echo off

SET Version=Net35
SET Config=Debug
CALL :COPYLIB
SET Config=Release
CALL :COPYLIB

SET Version=Net40
SET Config=Debug
CALL :COPYLIB
SET Config=Release
CALL :COPYLIB

SET Version=Net45
SET Config=Debug
CALL :COPYLIB
SET Config=Release
CALL :COPYLIB

pause

:COPYLIB
xcopy Reference\SuperSocket\%Version%\%Config%\SuperSocket.Facility.* bin\%Version%\%Config% /S /Y
xcopy Reference\SuperSocket\%Version%\%Config%\SuperSocket.SocketEngine.* bin\%Version%\%Config% /S /Y
xcopy Reference\SuperSocket\%Version%\%Config%\SuperSocket.SocketService.* bin\%Version%\%Config% /S /Y
xcopy Reference\SuperSocket\%Version%\%Config%\*.bat bin\%Version%\%Config% /S /Y
xcopy Reference\SuperSocket\%Version%\%Config%\Config bin\%Version%\%Config%\Config\ /S /Y
goto :eof

