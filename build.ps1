dotnet restore

$env:WORKSPACEROOT = "./test/DotNet.Globals.Core.Tests"

dotnet test $env:WORKSPACEROOT -c Release -f netcoreapp1.0
