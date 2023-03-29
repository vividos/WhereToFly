@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2023 Michael Fink
REM
REM Runs SonarCloud analysis build
REM

REM set this to your Visual Studio installation folder
set VSINSTALL=%ProgramFiles%\Microsoft Visual Studio\2022\Community

REM set this to your OpenCover executable
set OPENCOVER="C:\Projekte\Tools\OpenCover\OpenCover.Console.exe"

REM set this to your ReportGenerator executable
set REPORTGENERATOR="C:\Projekte\Tools\ReportGenerator\ReportGenerator.exe"

REM
REM Preparations
REM
call "%VSINSTALL%\Common7\Tools\VsDevCmd.bat"

set DOTNET_CLI_TELEMETRY_OPTOUT=1

REM install SonarScanner, if not available yet
dotnet tool update --global dotnet-sonarscanner

if "%SONARLOGIN%" == "" echo "Environment variable SONARLOGIN is not set! Obtain a new token and set the environment variable!"
if "%SONARLOGIN%" == "" exit 1

set VSTEST=%VSINSTALL%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

REM
REM Build using SonarQube scanner for MSBuild
REM
rmdir .\bw-output /s /q 2> nul

dotnet-sonarscanner begin ^
    /k:"WhereToFly" ^
    /v:"1.14.1" ^
    /d:"sonar.cs.opencover.reportsPaths=%CD%\TestResults\WhereToFly-*-CoverageReport.xml" ^
    /d:"sonar.exclusions=Web\LiveTracking\wwwroot\lib\**\*" ^
    /d:"sonar.host.url=https://sonarcloud.io" ^
    /o:"vividos-github" ^
    /d:"sonar.login=%SONARLOGIN%"
if errorlevel 1 goto end

REM
REM Rebuild projects
REM
msbuild WhereToFly.sln /m /property:Configuration=SonarQube /target:Restore;Rebuild

REM
REM Run Unit-Tests
REM
%OPENCOVER% ^
    -register:user ^
    -target:"%VSTEST%" ^
    -targetargs:"\"%~dp0App\UnitTest\bin\Release\net48\WhereToFly.App.UnitTest.dll\"" ^
    -filter:"+[WhereToFly*]* -[WhereToFly.App.Android]* -[WhereToFly.App.UnitTest]*" ^
    -mergebyhash ^
    -skipautoprops ^
    -output:"%~dp0\TestResults\WhereToFly-App-CoverageReport.xml"

%OPENCOVER% ^
    -register:user ^
    -target:"c:\Program Files\dotnet\dotnet.exe" ^
    -targetdir:"%~dp0WebApi\UnitTest\\" ^
    -targetargs:"test -c Release" ^
    -filter:"+[WhereToFly*]* -[WhereToFly.WebApi.UnitTest]*" ^
    -mergebyhash ^
    -skipautoprops ^
    -oldstyle ^
    -output:"%~dp0\TestResults\WhereToFly-WebApi-CoverageReport.xml"

%REPORTGENERATOR% ^
    -reports:"%~dp0\TestResults\WhereToFly-*-CoverageReport.xml" ^
    -targetdir:"%~dp0\TestResults\CoverageReport"

dotnet-sonarscanner end /d:"sonar.login=%SONARLOGIN%"

:end

pause
