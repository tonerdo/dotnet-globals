#!/usr/bin/env bash

echo "Installing dotnet-globals..."

application_folder="/usr/local/"
version="1.0.1-rc1"
temp_file="/tmp/dotnet-globals.tar.gz"
unzipped_folder="/tmp/dotnet-globals"

if [ -f $temp_file ]; then
  rm $temp_file
fi

if [ -d $unzipped_folder ]; then
  rm -rf $unzipped_folder
fi

download_uri=https://github.com/tsolarin/dotnet-globals/releases/download/v$version/dotnet-globals.tar.gz
download_file=$temp_file

echo "Downloading from $download_uri..."

curl -o $download_file -L $download_uri
tar -xvf $download_file -C /tmp

if [ -d "$application_folder/dotnet-globals" ]; then
  rm -rf "$application_folder/dotnet-globals"
fi

mv $unzipped_folder $application_folder

symlink="/usr/local/bin/dotnet-globals"
if [ -f $symlink ]; then
  rm $symlink
fi

ln -s "$application_folder/dotnet-globals/dotnet-globals" /usr/local/bin

echo "Installation complete. Add $HOME/.dotnet-globals/bin to your PATH"
