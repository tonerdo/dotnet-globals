#!/usr/bin bash

set -e

dotnet restore

export WORKSPACEROOT="./test/DotNet.Globals.Core.Tests"

dotnet test $WORKSPACEROOT -c Release -f netcoreapp1.0

