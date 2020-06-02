@echo off
:start
dotnet run --urls="http://localhost:5000"
pause
goto start