dotnet publish --configuration Release --framework netcoreapp3.1 /p:PublishSingleFile=true --runtime win-x64 --output wol64
dotnet publish --configuration Release --framework netcoreapp3.1 /p:PublishSingleFile=true --runtime win-x86 --output wol86

dotnet publish --configuration Release --framework net48 --output net48

PAUSE