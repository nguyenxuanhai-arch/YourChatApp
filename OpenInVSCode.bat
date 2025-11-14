@echo off
REM Open Visual Studio Code with the current workspace

echo Opening YourChatApp in Visual Studio Code...

if command -v code >nul 2>&1 (
    code .
    echo Opened in Visual Studio Code!
) else (
    echo Visual Studio Code is not installed or not in PATH
    echo Please install Visual Studio Code from https://code.visualstudio.com/
)

pause
