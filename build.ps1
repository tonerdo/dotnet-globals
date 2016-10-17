param(
    [string]$p1 = "Debug"
)

dotnet restore

$env:WORKSPACEROOT = ".\test\DotNet.Globals.Core.Tests"

dotnet build ".\src\DotNet.Globals.Core" -c $p1
dotnet build ".\src\DotNet.Globals.Cli" -c $p1
dotnet test $env:WORKSPACEROOT -c $p1
