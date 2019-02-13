# Development

## Requirements

Dotnetcore 2.2

Note that Visual Studio is quirky with Dotnetcore - you probably want to run the latest version of it, and make sure you have the Dotnetcore 2.2 SDK installed (not just the runtime).


## Project status

To-do list

    - UI improvements
    - Tests
    - Documentation 
    - Security and permissions
    - Integrity self-checks
    - Self-repairing of index data
    - Support for tarballs
    - Post-commit hooks
    - Ability to receive new builds while file index is disconnected for maintenance.


## Running from command line 

You can of course run everything from Visual Studio, but if you're using the command line, read on.

Opening for the first time? Run

    cd /src
    dotnet restore

Start app

    cd /src
    dotnet build
    dotnet run

to view navigate your browser to

    http://localhost:3000/

## Build for deploy

from command line

    cd /src/Tetri.Web
    dotnet publish /property:PublishWithAspNetCoreTargetManifest=false

Your build artefacts will be in /src/Tetri.Web/bin/Debug/netcoreapp2.2/publish/
To start the server run the following from the same folder as the build arfecats

    dotnet run Tetri.Web.dll

## Test

From command line

    cd /src
    dotnet test

from visual studio, open Test > Windows > Test Explorer, run tests.

External tests : first start the server from another process. Then

    cd Tetri.ExternalTests
    dotnet build
    dotnet run