if not exist "%~dp0publish" mkdir "%~dp0publish"

set "OutputPath=%~dp0publish\TrainDatasetGenerator"

rmdir "%OutputPath%" /s /q

mkdir "%OutputPath%"

cd %~dp0TrainDatasetGenerator
dotnet publish --output "%OutputPath%" --configuration release --self-contained true -r win-x64

pause;