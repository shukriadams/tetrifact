# Development

## Requirements

- Dotnetcore 2.2 SDK
- Visual Studio 2017 (Windows) or Visual Studio Code with C# extension. If you're using Visual Studio make sure you've update to the latest version.

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

## Running from Visual Studio

Set Tetrifact.Web as your start project and run in IIS Express.

## Running from command line 

Opening for the first time? Run

    cd /src
    dotnet restore

Start app

    cd /src
    dotnet build
    dotnet run --project Tetrifact.Web

to view navigate your browser to

    http://localhost:3000/

All content is placed in /src/Tetrifact.Web/bin/Debug/netcoreapp2.2/data

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

## Architecture

This is a brief explanation of Tetrifact's structure and concepts

### Tags

Packages can be tagged with strings to aid identification. Tags are stored in manifest.json of a given package, but are also written to the /tags folder. The latter is used as a fast index to effeciently list all tags without having to iterate and load every manifest. Tags in this folder are base64 encoded to allow tags to contain characters unsupported by the filesystem. 

The source of truth for tags are manifest files, the /tags folder is secondary and can be rebuilt at any time should they become corrupted. If a tag folder is directly renamed so its text is not a valid base64 string, it will not appear in tag read lists.