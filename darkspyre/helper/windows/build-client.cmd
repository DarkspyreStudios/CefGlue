@echo off
rem Compiles the Windows CEF client DLL (client.c) for one runtime identifier, from a Visual Studio developer
rem command prompt for that architecture (vcvars64 or vcvarsamd64_arm64).
rem
rem Usage: build-client.cmd <rid> <cef distribution folder> <output folder>
rem   <rid>   win-x64 or win-arm64.
rem   Output: <output folder>\<rid>\darkspyre-chromium-service.dll and darkspyre-chromium-service.exe, the
rem   distribution's bootstrapc.exe renamed so it loads the DLL of the same base name.
rem
rem CEF_API_VERSION must equal CEF_API_VERSION in CefGlue.Interop.Gen\gen-cef3.sh.
setlocal
if "%~3"=="" (
  echo Usage: build-client.cmd ^<rid^> ^<cef distribution folder^> ^<output folder^>
  exit /b 2
)
set RID=%~1
set DIST=%~f2
set OUT=%~f3\%RID%
set CEF_API_VERSION=15600
if not exist "%DIST%\include\cef_sandbox_win.h" (
  echo %DIST% is not a CEF distribution.
  exit /b 1
)
if not exist "%OUT%" mkdir "%OUT%"
cl /nologo /LD /O2 /W4 /WX /DCEF_API_VERSION=%CEF_API_VERSION% /I"%DIST%" "%~dp0client.c" ^
  /Fe"%OUT%\darkspyre-chromium-service.dll" /Fo"%OUT%\\" /link shell32.lib || exit /b 1
copy /y "%DIST%\Release\bootstrapc.exe" "%OUT%\darkspyre-chromium-service.exe" >nul || exit /b 1
echo Built %OUT%
