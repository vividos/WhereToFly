#!/usr/bin/env bash
# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

# build the weblib npm package
pushd ../../Shared/WebLib
npm install
npm run build-release
popd
