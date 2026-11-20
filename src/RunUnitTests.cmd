@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2026 Michael Fink
REM
REM Runs Unit tests and coverage analysis
REM
echo RunUnitTests.cmd - Runs unit tests and generates coverage report
echo.

REM
REM Preparations
REM

set DOTNET_CLI_TELEMETRY_OPTOUT=1

REM install ReportGenerator, if not available yet
dotnet tool install dotnet-reportgenerator-globaltool --tool-path .dotnet-tools

set PATH=%PATH%;"%CD%\.dotnet-tools"

mkdir "%~dp0\TestResults" 2> nul

REM
REM Run Unit-Tests
REM
dotnet test ^
    App\UnitTest\WhereToFly.App.UnitTest.csproj ^
    --configuration Release ^
    --results-directory TestResults ^
    --output Detailed ^
    --report-trx ^
    --report-trx-filename WhereToFly-App-Log.xml ^
    --coverlet ^
    --coverlet-output-format opencover

dotnet test ^
    Geo\UnitTest\WhereToFly.Geo.UnitTest.csproj ^
    --configuration Release ^
    --results-directory TestResults ^
    --output Detailed ^
    --report-trx ^
    --report-trx-filename WhereToFly-Geo-Log.trx ^
    --coverlet ^
    --coverlet-output-format opencover

dotnet test ^
    WebApi\UnitTest\WhereToFly.WebApi.UnitTest.csproj ^
    --configuration Release ^
    --results-directory TestResults ^
    --output Detailed ^
    --report-trx ^
    --report-trx-filename LogFileName=WhereToFly-WebApi-Log.trx ^
    --coverlet ^
    --coverlet-output-format opencover

ReportGenerator ^
    -reports:TestResults\**\*.opencover.xml ^
    -targetdir:"%~dp0\TestResults\CoverageReport"

pause
