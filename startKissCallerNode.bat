@echo off
set host=kissgen.com
set port=3000
set socketIoConsoleExe="C:\MyCode\KissCaller\KissCaller.SocketIo\bin\Debug\KissCaller.SocketIo.exe"
@echo on

start "node cmd" /B node KissCaller.NodeJs/kissCaller.js --host %host% --port %port%

pause
