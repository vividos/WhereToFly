@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2026 Michael Fink
REM
REM Runs UI tests on Android devices
REM
echo RunAndroidUITests.cmd - Runs UI tests on Android devices
echo.

REM
REM Preparations
REM

set DOTNET_CLI_TELEMETRY_OPTOUT=1

REM
REM Build and run UI Tests
REM
dotnet test ^
    App\UITest\WhereToFly.App.UITest.csproj ^
    --configuration Debug ^
    --environment UITEST_PLATFORM=Android ^
    --environment UITEST_APPID=de.vividos.app.wheretofly.android_dbg ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-App-UITest-Log.xml ^
    --logger:console;verbosity=detailed

pause
