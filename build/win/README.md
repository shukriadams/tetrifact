
Builds Tetrifact on a host Windows system

## Requirements

- Powershell
- Dotnetcore 2.2 SDK
- git

## Build

At the Powershell command line run

    powershell -ExecutionPolicy ByPass .\build.ps1 -tag XYZ

where XYZ is the existing git tag to build. 

The resulting build will be placed in .\out in a zip archive called Tetrifact.XYZ.zip. 

The source artefacts can be found in clone\src\Tetrifact.Web\bin\Release\netcoreapp2.2\publish

## Run

To start the server from the built binaries run

    dotnet Tetrifact.Web.dll
