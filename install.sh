#!/usr/bin/env bash

echo "Installing dotnet-globals..."

application_folder="/usr/local/dotnet-globals"
version="1.0.1-rc1"
version_folder="$application_folder/$version"

if [ ! -d $application_folder ]; then
  mkdir $application_folder
fi

if [ -d $version_folder ]; then
  echo "dotnet-globals $version already installed"
  exit
fi

download_uri=https://github.com/tsolarin/dotnet-globals/releases/download/v$version/dotnet-globals.tar.gz
download_file="$application_folder/dotnet-globals.tar.gz"

echo "Downloading from $download_uri..."

curl -o $download_file -L $download_uri
tar -xvzf $download_file -C $application_folder

unzipped_folder="$application_folder/dotnet-globals"
mv $unzipped_folder $version_folder

symlink="/usr/local/bin/dotnet-globals"
if [ -f $symlink ]; then
  rm $symlink
fi

ln -s "$version_folder/dotnet-globals" /usr/local/bin

echo "Installation complete. Add $HOME/.dotnet-globals/bin to your PATH"
