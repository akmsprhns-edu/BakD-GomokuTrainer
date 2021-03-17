if not exist "%~dp0publish" mkdir "%~dp0publish"

set "PythonScriptsPath=%~dp0publish\PythonScripts"

rmdir "%PythonScriptsPath%" /s /q

mkdir "%PythonScriptsPath%"

cd %~dp0PythonScripts
robocopy . "%PythonScriptsPath%" *.py

pause;