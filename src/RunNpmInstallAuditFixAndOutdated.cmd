@echo off
REM
REM Where-to-fly - an app to decide where to (hike up and) fly with a paraglider
REM Copyright (C) 2017-2026 Michael Fink
REM
REM Runs "npm install", "npm audit fix" and then "npm outdated" on npm projects
REM
echo RunNpmInstallAndOutdated.cmd - Runs npm install, audit fix and outdated
echo.

set PROJECTS=^
   "Shared\WebLib" ^
   "Web\LiveTracking\Frontend" ^
   "Web\App\Frontend"

for %%P in (%PROJECTS%) do (
   echo Project %%P...
   pushd %%P
   cmd /c npm install
   cmd /c npm audit fix
   cmd /c npm outdated
   popd
   echo.
)

pause
