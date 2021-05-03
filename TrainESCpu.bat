@echo off
set "PythonScriptsPath=%~dp0publish\PythonScripts"
set "EstimatorExePath=%~dp0publish\EstimatorExe"
set /p IterationCount=Iteration Count: 

@echo on
start "Training..." /low /wait call "python" "%PythonScriptsPath%\gomokutrainer.py" %IterationCount% "C:\Projects\GomokuTrainer\Output" "%EstimatorExePath%\OnnxEstimatorCpu.exe"

pause;