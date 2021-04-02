if not exist "%~dp0publish" mkdir "%~dp0publish"

set "PythonScriptsPath=%~dp0publish\PythonScripts"
set "EstimatorExePath=%~dp0publish\EstimatorExe"

rmdir "%EstimatorExePath%" /s /q
rmdir "%PythonScriptsPath%" /s /q

mkdir "%EstimatorExePath%"
mkdir "%PythonScriptsPath%"

cd %~dp0OnnxEstimatorGpu
dotnet publish --output "%EstimatorExePath%" --configuration release --self-contained true -r win-x64
cd %~dp0PythonScripts
robocopy . "%PythonScriptsPath%" *.py

pause;