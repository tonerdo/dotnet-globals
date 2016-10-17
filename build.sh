#!/usr/bin/env bash

set -e

if [[ !$1 ]]; then
    CONFIGURATION="Debug"
fi

if [[ $1 ]]; then
    CONFIGURATION=$1
fi

dotnet restore

export WORKSPACEROOT="./test/DotNet.Globals.Core.Tests"

dotnet build "./src/DotNet.Globals.Core" -c $CONFIGURATION
dotnet build "./src/DotNet.Globals.Cli" -c $CONFIGURATION
dotnet test $WORKSPACEROOT -c $CONFIGURATION
