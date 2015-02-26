PUSHD "%~dp0"
IF NOT EXIST .nuget MD .nuget || PAUSE
IF NOT EXIST .nuget\NuGet.exe PowerShell.exe -Command "Invoke-WebRequest http://nuget.org/nuget.exe -OutFile .nuget\NuGet.exe" || PAUSE

.nuget\NuGet.exe pack -o .. || PAUSE
POPD
