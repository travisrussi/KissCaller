@echo off
set host=kissgen.com
set port=3000
set socketIoConsoleExe="C:\MyCode\KissCaller\KissCaller.SocketIo\bin\Debug\KissCaller.SocketIo.exe"
@echo on
%socketIoConsoleExe% "http://www.%host%:%port%"
pause
