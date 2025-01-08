@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2025 Michael Fink
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
    --collect:"XPlat Code Coverage;Format=opencover" ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-App-Log.xml ^
    --logger:console;verbosity=detailed

dotnet test ^
    App\Svg\UnitTest\WhereToFly.App.Svg.UnitTest.csproj ^
    --configuration Release ^
    --collect:"XPlat Code Coverage;Format=opencover" ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-App-Svg-Log.xml ^
    --logger:console;verbosity=detailed

dotnet test ^
    Geo\UnitTest\WhereToFly.Geo.UnitTest.csproj ^
    --configuration Release ^
    --collect:"XPlat Code Coverage;Format=opencover" ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-Geo-Log.trx ^
    --logger:console;verbosity=detailed

dotnet test ^
    WebApi\UnitTest\WhereToFly.WebApi.UnitTest.csproj ^
    --configuration Release ^
    --collect:"XPlat Code Coverage;Format=opencover" ^
    --results-directory TestResults ^
    --logger:trx;LogFileName=WhereToFly-WebApi-Log.trx ^
    --logger:console;verbosity=detailed

ReportGenerator ^
    -reports:TestResults\**\*.opencover.xml ^
    -targetdir:"%~dp0\TestResults\CoverageReport"

pause
