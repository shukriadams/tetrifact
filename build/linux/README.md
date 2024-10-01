
Builds Tetrifact on a host Linux system

## Requirements

- bash
- net6.0 SDK
- git

## Run

    sh ./build.sh XYZ

where XYZ is the existing git tag to build. 

The resulting build will be placed in .\out in a tar/gz archive called Tetrifact.XYZ.tar.gz

The source artefacts can be found in clone\src\Tetrifact.Web\bin\Release\net6.0\publish

## Run

To start the server from the built binaries run

    dotnet Tetrifact.Web.dll
