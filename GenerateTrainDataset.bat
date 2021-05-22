@echo off
set "ExePath=%~dp0publish\TrainDatasetGenerator\TrainDatasetGenerator.exe"

@echo on
start "Generating dataset..." /wait call "%ExePath%" "C:\Projects\TrainDatasetGenerator"

pause;