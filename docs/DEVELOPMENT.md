# Development

## Requirements

Dotnetcore 2.2

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


to scaffold new app

    dotnet restore
    dotnet build
    dotnet run

to view

    http://localhost:3000/

from command line

    cd Tetri.Web
    dotnet run Tetri.Web/bin/Debug/netcoreapp2.2/Tetri.Web.dll

to test

from command line

    cd /src
    dotnet test

from visual studio, open Test > Windows > Test Explorer, run tests.

External tests : first start the server from another process. Then

    cd Tetri.ExternalTests
    dotnet build
    dotnet run