@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2025 Michael Fink
REM
REM Publishes the AOT test project
REM
echo PublishAotTest.cmd - Publishes the AOT test project
echo.

REM
REM Preparations
REM

set DOTNET_CLI_TELEMETRY_OPTOUT=1

REM
REM Publish AOT test
REM
dotnet publish ^
    App/AotTest/WhereToFly.App.AotTest.csproj ^
    -c Release ^
    -f net9.0

pause
