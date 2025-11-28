# Development

Note : if all you want is to build Tetrifact from source, check the /build folder in the project root.

## Conventions

    - all file endings are LF, NOT CRLF.
    - indent with 4 spaces.

## Vagrant

If you want to develop Linux on Windows, Vagrant is an excellent and convenient option for getting the best of both worlds. The how and why of Vagrant is beyond the scope of this document, check out https://www.vagrantup.com for more info. The full Vagrant setup for this project is in the /vagrant folder. 

## Requirements

- Dotnet 6.0 SDK
- Visual Studio 2019 (Windows) or Visual Studio Code with C# extension. If you're using Visual Studio make sure you've update to the latest version. Visual Studio 2017 isn't supported.

Unit test coverage is done with https://github.com/SteveGilham/altcover, install ReportGenerator. To get a version compatible with this project run
    
    dotnet tool install --global dotnet-reportgenerator-globaltool --version 5.3.11

## Running from Visual Studio

- Start Visual Studio as administrator
- Open /src/Tetrifact.sln
- Set Tetrifact.Web as your start project and run in IIS Express.

To access your solution from another PC, open `<project root>/src/.vs/Tetrifact/config/applicationhost.config` and find all instances of `<binding protocol="http" bindingInformation="*:<PORT>:localhost" />` and replace `localhost` with `*`. There should be two instances, for ports `7313` and `8080`, but this might vary for your system.

Visual Studio performs poorly with large builds. If you want to work with very large uploads (10K+ files, 10+ gigs of data), try running Tetrifact from the command line instead.

## Running from command line 

Opening for the first time? Run

    cd /src
    dotnet restore

Start app

    cd /src
    dotnet build
    dotnet run --project Tetrifact.Web

to view navigate your browser to

    http://localhost:5001/

All content is placed in /src/Tetrifact.Web/bin/Debug/net6.0/data

## Build for deploy

from command line

    cd /src
    dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release

Your build artefacts will be in /src/Tetrifact.Web/bin/Debug/net6.0/publish/
To start the server run the following from the same folder as the build artefacts

    dotnet Tetrifact.Web.dll

## Test

To run tests in a container (requires bash && Docker)

    cd /tests
    sh ./test.sh

This is mostly intended for running tests on CI systems like Travis. To run tests natively from the command line (requires dotnetcore sdk)  run

    cd /src
    dotnet test /p:AltCover=true

To run tests in Visual Studio, open Test > Windows > Test Explorer, run desired tests from explorer window.

## Test coverage reports

After testing run 

    cd /src

    reportgenerator -reports:./Tetrifact.Tests/coverage.xml -targetdir:./Tetrifact.Tests/coverage -assemblyfilters:+Tetrifact.*;-Tetrifact.Tests -classfilters:-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*

The HTML report is at

    /src/Tetrifact.Tests/coverage/index.html

You can also run /src/cover.sh or cover.bat, either will test and cover all-in-one.

## Architecture

This is a brief explanation of Tetrifact's structure and concepts

### Package

A package is a conceputal collection of one or more files that are added to Tetrifact. A package is expected to have a unique name, assigned at creation, and this name will be used by you the user to retrieve the package again. Normally in a continuous integration setup, name corresponds one-to-one with a build id, which in turn usually corresponds with the version control revision id or hash from which the build was done. 

### Manifests

A Manifest is a JSON file that lists all the files in a package, along with metadata describing the package. The manifest contains a collection of file paths that point to the files in a package. 

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
