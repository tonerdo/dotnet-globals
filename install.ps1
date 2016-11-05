Write-Host "Installing dotnet-globals..."

$application_folder = "$Env:APPDATA"
$version = "1.0.1-rc1"
$temp_file = "$Env:TEMP\dotnet-globals.zip"
$unzipped_folder = "$Env:TEMP\dotnet-globals"

if (Test-Path $temp_file) {
    Remove-Item $temp_file -type file
}

if (Test-Path $unzipped_folder) {
    Remove-Item $unzipped_folder -type directory
}

$download_uri = https://github.com/tsolarin/dotnet-globals/releases/download/v$version/dotnet-globals.zip
$download_file = $temp_file

Write-Host "Downloading from $download_uri..."

Invoke-WebRequest -UseBasicParsing -uri $download_uri -OutFile $download_file
Add-Type -AssemblyName System.IO.Compression.FileSystem

[System.IO.Compression.ZipFile]::ExtractToDirectory($download_file, $Env:TEMP)

if (Test-Path "$application_folder\dotnet-globals") {
    Remove-Item "$application_folder\dotnet-globals" -type directory
}

Move-Item $unzipped_folder $application_folder

[Environment]::SetEnvironmentVariable("Path", $Env:Path + ";$application_folder\dotnet-globals", [EnvironmentVariableTartget]::User)
[Environment]::SetEnvironmentVariable("Path", $Env:Path + ";$Env:USERPROFILE\.dotnet-globals\bin", [EnvironmentVariableTartget]::User)

Write-Host "Installation complete."
