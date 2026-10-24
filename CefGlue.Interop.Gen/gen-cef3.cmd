@echo off
:: Determine python path by inspecting registry: HKCU/HKLM SOFTWARE\Python\PythonCore\${PYTHON_VERSION}\InstallPath
:: And then try some default locations: c:\python27amd64 , c:\python27
:: Keep CEF_API_VERSION equal to the value in gen-cef3.sh.
set CEF_API_VERSION=15600
python -B cefglue_interop_gen.py --cpp-header-dir include --cefglue-dir ..\CefGlue\ --api-version %CEF_API_VERSION% --no-backup
pause
