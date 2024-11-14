@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2023-2024 Michael Fink
REM
REM Cleans all temporary files and folders, including bin and obj filders
REM
echo Cleaning all temporary files and folders...
echo.

echo Removing "artifacts" folder...
rmdir /S /Q ..\artifacts 2> nul

echo Removing ".dotnet-tools" folder...
rmdir /S /Q .dotnet-tools 2> nul

echo Removing ".sonarqube" folder...
rmdir /S /Q .sonarqube 2> nul
echo.

echo Removing "bin" and "obj" folders in...
for /R %%A in (*.esproj) do (
    echo %%~dpA
    rmdir /S /Q %%~dpA\bin\ 2> nul
    rmdir /S /Q %%~dpA\obj\ 2> nul
)
echo.

echo Removing "node_modules" folder in...
for /R %%A in (.) do if exist "%%~fA\node_modules" (
    echo %%~fA
    rmdir /S /Q %%~fA\node_modules\ 2> nul
)
echo.

echo Removing npm projects' dist folders
rmdir /S /Q Shared\WebLib\dist 2> nul
rmdir /S /Q Web\LiveTracking\wwwroot 2> nul
echo.

echo Removing ".vs" folder in...
for /R %%A in (.) do if exist %%~dpA\.vs (
    echo %%~dpA
    rmdir /S /Q %%~dpA\.vs\ 2> nul
)
echo.
