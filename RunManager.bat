@echo off
REM Quick build and run script for YourChatApp

echo ╔════════════════════════════════════════════════╗
echo ║     YourChatApp - Build and Run Manager        ║
echo ╚════════════════════════════════════════════════╝
echo.

:menu
echo Select an option:
echo 1. Build entire solution
echo 2. Build Server only
echo 3. Build Client only
echo 4. Run Server
echo 5. Run Client
echo 6. Clean all build artifacts
echo 7. Open in Visual Studio
echo 8. Exit
echo.
set /p choice="Enter your choice (1-8): "

if "%choice%"=="1" goto build_all
if "%choice%"=="2" goto build_server
if "%choice%"=="3" goto build_client
if "%choice%"=="4" goto run_server
if "%choice%"=="5" goto run_client
if "%choice%"=="6" goto clean_all
if "%choice%"=="7" goto open_vs
if "%choice%"=="8" goto end
echo Invalid choice!
goto menu

:build_all
echo Building entire solution...
dotnet build
echo Build complete!
pause
goto menu

:build_server
echo Building Server...
cd Server
dotnet build
cd ..
echo Server build complete!
pause
goto menu

:build_client
echo Building Client...
cd Client
dotnet build
cd ..
echo Client build complete!
pause
goto menu

:run_server
echo Starting Server...
cd Server
dotnet run
cd ..
pause
goto menu

:run_client
echo Starting Client...
cd Client
dotnet run
cd ..
pause
goto menu

:clean_all
echo Cleaning all build artifacts...
for /d /r . %%d in (bin, obj) do @if exist "%%d" rd /s /q "%%d"
echo Clean complete!
pause
goto menu

:open_vs
echo Opening in Visual Studio...
if exist "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" (
    start "" "C:\Program Files\Microsoft Visual Studio\2022\Community\Common7\IDE\devenv.exe" "YourChatApp.sln"
) else if exist "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe" (
    start "" "C:\Program Files\Microsoft Visual Studio\2022\Professional\Common7\IDE\devenv.exe" "YourChatApp.sln"
) else (
    echo Visual Studio 2022 not found!
)
pause
goto menu

:end
echo Goodbye!
exit /b 0
