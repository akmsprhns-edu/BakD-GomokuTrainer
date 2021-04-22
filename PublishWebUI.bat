if not exist "%~dp0publish" mkdir "%~dp0publish"

set "WebUIPath=%~dp0publish\WebUI"

rmdir "%WebUIPath%" /s /q

mkdir "%WebUIPath%"

cd %~dp0GomokuWebUI
dotnet publish --output "%WebUIPath%" --configuration release --self-contained true -r win-x64

pause;