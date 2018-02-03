@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2018 Michael Fink
REM
REM Runs SonarCloud analysis build
REM

REM set this to your Visual Studio installation folder
set VSINSTALL=%ProgramFiles(x86)%\Microsoft Visual Studio\2017\Community

REM set this to your SonarQube tools folder
set SONARQUBE=D:\devel\tools\SonarQube

REM set this to your OpenCover executable
set OPENCOVER="d:\devel\tools\OpenCover\OpenCover.Console.exe"

REM set this to your ReportGenerator executable
set REPORTGENERATOR="D:\devel\tools\ReportGenerator\ReportGenerator.exe"

REM
REM Preparations
REM
call "%VSINSTALL%\Common7\Tools\VsDevCmd.bat"

set PATH=%PATH%;%SONARQUBE%\build-wrapper-win-x86;%SONARQUBE%\sonar-scanner-msbuild

if "%SONARLOGIN%" == "" echo "Environment variable SONARLOGIN is not set! Obtain a new token and set the environment variable!"
if "%SONARLOGIN%" == "" exit 1

set VSTEST=%VSINSTALL%\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe

REM
REM Build using SonarQube scanner for MSBuild
REM
rmdir .\bw-output /s /q 2> nul

SonarQube.Scanner.MSBuild.exe begin ^
    /k:"WhereToFly" ^
    /v:"1.1.0" ^
    /d:"sonar.cfamily.build-wrapper-output=%CD%\bw-output" ^
    /d:"sonar.cs.opencover.reportsPaths=%CD%\TestResults\WhereToFly-CoverageReport.xml" ^
    /d:"sonar.host.url=https://sonarcloud.io" ^
    /d:"sonar.organization=vividos-github" ^
    /d:"sonar.login=%SONARLOGIN%"

REM
REM Rebuild project
REM
build-wrapper-win-x86-64.exe --out-dir bw-output msbuild WhereToFly.sln /m /property:Configuration=Release /target:Rebuild

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

SonarQube.Scanner.MSBuild.exe end /d:"sonar.login=%SONARLOGIN%"

pause
