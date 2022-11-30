#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

# build the weblib npm package
pushd ../../Shared/WebLib
npm install
npm run build-release
popd

# also build the MapView project before building the Android app
msbuild ../MapView/WhereToFly.App.MapView.csproj /restore /t:Build /p:Configuration=Release
