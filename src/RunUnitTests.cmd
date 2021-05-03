@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2021 Michael Fink
REM
REM Runs Unit tests and coverage analysis
REM

REM set this to your Visual Studio installation folder
set VSINSTALL=%ProgramFiles(x86)%\Microsoft Visual Studio\2019\Community

REM set this to your OpenCover executable
set OPENCOVER="C:\Projekte\Tools\OpenCover\OpenCover.Console.exe"

REM set this to your ReportGenerator executable
set REPORTGENERATOR="C:\Projekte\Tools\ReportGenerator\ReportGenerator.exe"

REM
REM Preparations
REM
call "%VSINSTALL%\Common7\Tools\VsDevCmd.bat"

set DOTNET_CLI_TELEMETRY_OPTOUT=1

set VSTEST=%VSINSTALL%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

REM
REM Build projects
REM
msbuild App\UnitTest\WhereToFly.App.UnitTest.csproj /m /property:Configuration=Release /target:Build

pushd "%~dp0WebApi\UnitTest\"
dotnet build -c Release
popd

REM
REM Run Unit-Tests
REM
%OPENCOVER% ^
    -register:user ^
    -target:"%VSTEST%" ^
    -targetargs:"\"%~dp0App\UnitTest\bin\Release\net462\WhereToFly.App.UnitTest.dll\"" ^
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

pause
