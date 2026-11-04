@echo off
setlocal
set "GAME_EXE=%~dp0builds\windows\BusinessSimulator-Windows-x64.exe"
if exist "%~dp0BusinessSimulator-Windows-x64.exe" set "GAME_EXE=%~dp0BusinessSimulator-Windows-x64.exe"
if exist "%GAME_EXE%" (
  start "" "%GAME_EXE%" %*
  exit /b 0
)
echo Descarca BusinessSimulator-Windows-x64.exe din pagina care se deschide.
echo Apoi deschide fisierul descarcat. Nu trebuie instalat Godot sau .NET.
start "" "https://github.com/Chuvachox/BusinessSimulator/releases/tag/v0.3.0-alpha"
pause
exit /b 0
