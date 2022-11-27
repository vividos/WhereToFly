# App Center custom build scripts: https://aka.ms/docs/build/custom/scripts

export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_NOLOGO=true
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true

# Update Mono and Xamarin.Android using Boots global tool
dotnet tool install --global boots

boots --preview Mono
boots --preview Xamarin.Android
