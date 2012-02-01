@echo off
set host=kissgen.com
set port=3000
@echo on

start "node cmd" /B node KissCaller.NodeJs/kissCaller.js --host %host% --port %port%

pause
