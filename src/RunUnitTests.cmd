@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2023 Michael Fink
REM
REM Runs Unit tests and coverage analysis
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

mkdir "%~dp0\TestResults" 2> nul

set DOTNET_CLI_TELEMETRY_OPTOUT=1

set VSTEST=%VSINSTALL%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

REM
REM Build projects
REM
dotnet publish App\UnitTest\WhereToFly.App.UnitTest.csproj --configuration Release

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

dotnet test ^
    WebApi\UnitTest\WhereToFly.WebApi.UnitTest.csproj ^
    --configuration Release ^
    --collect:"XPlat Code Coverage;Format=opencover" ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-WebApi-Log.trx ^
    --logger:console;verbosity=detailed

%REPORTGENERATOR% ^
    -reports:TestResults\WhereToFly-*-CoverageReport.xml;TestResults\**\*.opencover.xml ^
    -targetdir:"%~dp0\TestResults\CoverageReport"

pause
