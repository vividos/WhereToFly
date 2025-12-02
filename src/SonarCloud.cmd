@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2025 Michael Fink
REM
REM Runs SonarCloud analysis build
REM

echo SonarCloud.cmd - Runs SonarCloud analysis build
echo.

set DOTNET_CLI_TELEMETRY_OPTOUT=1

REM install SonarScanner, if not available yet
dotnet tool install dotnet-sonarscanner --tool-path .dotnet-tools

set PATH=%PATH%;"%CD%\.dotnet-tools"

REM check for SONARLOGIN env var
if "%SONARLOGIN%" == "" echo "Environment variable SONARLOGIN is not set! Obtain a new token and set the environment variable!"
if "%SONARLOGIN%" == "" exit 1

REM
REM Build using SonarQube scanner for MSBuild
REM
rmdir .\bw-output /s /q 2> nul

dotnet-sonarscanner begin ^
    /k:"WhereToFly" ^
    /v:"1.17.0" ^
    /s:SonarQube.Analysis.xml ^
    /d:"sonar.cs.opencover.reportsPaths=%CD%\TestResults\**\*.opencover.xml" ^
    /d:"sonar.host.url=https://sonarcloud.io" ^
    /o:"vividos-github" ^
    /d:"sonar.token=%SONARLOGIN%"
if errorlevel 1 goto end

REM
REM Rebuild projects
REM
dotnet build WhereToFly.slnx -c Release

REM
REM Run Unit-Tests
REM
call RunUnitTests.cmd

dotnet-sonarscanner end /d:"sonar.token=%SONARLOGIN%"

:end

pause
