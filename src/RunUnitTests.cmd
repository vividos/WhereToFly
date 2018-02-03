@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2018 Michael Fink
REM
REM Runs Unit tests and coverage analysis
REM

REM set this to your Visual Studio installation folder
set VSINSTALL=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community

REM set this to your OpenCover executable
set OPENCOVER="d:\devel\tools\OpenCover\OpenCover.Console.exe"

REM set this to your ReportGenerator executable
set REPORTGENERATOR="D:\devel\tools\ReportGenerator\ReportGenerator.exe"

REM
REM Preparations
REM
call "%VSINSTALL%\Common7\Tools\VsDevCmd.bat"


set VSTEST=%VSINSTALL%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

REM
REM Rebuild project
REM
msbuild UnitTest\WhereToFly.UnitTest.csproj /m /property:Configuration=Release /target:Build

REM
REM Run Unit-Tests
REM
%OPENCOVER% ^
    -register:user ^
    -target:"%VSTEST%" ^
    -targetargs:"\"%~dp0UnitTest\bin\Release\WhereToFly.UnitTest.dll\"" ^
    -filter:"+[WhereToFly*]* -[WhereToFly.UnitTest]*" ^
    -mergebyhash ^
    -skipautoprops ^
    -output:"%~dp0\TestResults\WhereToFly-CoverageReport.xml"

%REPORTGENERATOR% ^
    -reports:"%~dp0\TestResults\WhereToFly-CoverageReport.xml" ^
    -targetdir:"%~dp0\TestResults\CoverageReport"

pause
