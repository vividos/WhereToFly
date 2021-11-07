#!/usr/bin/env bash

# remove the UWP project in AppCenter builds to prevent an error when trying
# to restore NuGet packages for it.
rm ../UWP/WhereToFly.App.UWP.csproj
