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

    cd /src
    dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release

Your build artefacts will be in /src/Tetrifact.Web/bin/Debug/netcoreapp2.2/publish/
To start the server run the following from the same folder as the build artefacts

    dotnet Tetrifact.Web.dll

## Test

To run tests in a container (requires bash && Docker)

    cd /tests
    sh ./test.sh

To run tests natively from the command line (requires dotnetcore sdk)

    cd /src
    dotnet test

To run tests in Visual Studio, open Test > Windows > Test Explorer, run desired tests from explorer window.

## Architecture

This is a brief explanation of Tetrifact's structure and concepts

### Package

A package is a collection of one or more files that are added to Tetrifact. A package is expected to have a unique name, assigned at creation, and this name will be used by you the user to retrieve the package again. Normally in a continuous integration setup, name corresponds one-to-one with a build id, which in turn usually corresponds with the version control revision id or hash from which the build was done. 

### Repository

The files in a package are stored in the repository directory, by default at /data/repository. A file added to Tetrifact has an absolute path within its package, and this corresponds to the path the file will have in the repository directory. A file isn't written directly into it's parent directory ; rather, it is nested within two additional directories - the first is a directory with the name (including extension) of the file,and then an additional folder named for the file's hash. The file itself is always named "bin" (no file extension). Next to file is a "packages" folder, which in turn contains a file for each package which contains that file at the hash.

For example, if you create a package called "123" with the file /diagrams/image.jpg" in it, and that file had a hash of 0f16cd, the repository would contain the following items

    data
    +-- repository
        +-- diagrams
            +-- image.jpg
                +-- 0f16cd
                    |-- packages
                    |    +-- 123
                    +-- bin

This simple filesystem-based indexing system allows packages to share a file if file's absolute path and hash are identical, without creating unnnecessary entanglement between packages. If a package is deleted, its file in the /packages folder is removed, but the binary of the file can remain if other packages still reference it.

### Tags

Packages can be tagged with strings to aid identification. Tags are stored in manifest.json of a given package, but are also written to the /tags folder. The latter is used as a fast index to effeciently list all tags without having to iterate and load every manifest. Tags in this folder are base64 encoded to allow tags to contain characters unsupported by the filesystem. 

The source of truth for tags are manifest files, the /tags folder is secondary and can be rebuilt at any time should they become corrupted. If a tag folder is directly renamed so its text is not a valid base64 string, it will not appear in tag read lists.