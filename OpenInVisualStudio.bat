@echo off
REM Script to open YourChatApp solution in Visual Studio 2022

echo Opening YourChatApp solution in Visual Studio 2022...

REM Check if Visual Studio 2022 is installed
for /f "tokens=*" %%a in ('where devenv.exe 2^>nul') do (
    set "VS_PATH=%%a"
    goto found
)

REM Try default Visual Studio 2022 path
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe"
    goto found
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe"
    goto found
)

if exist "C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe" (
    set "VS_PATH=C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\devenv.exe"
    goto found
)

echo Visual Studio 2022 not found!
echo Please install Visual Studio 2022 or add it to PATH
pause
exit /b 1

:found
echo Found Visual Studio at: %VS_PATH%
start "" "%VS_PATH%" "YourChatApp.sln"
echo Solution opened successfully!
exit /b 0
