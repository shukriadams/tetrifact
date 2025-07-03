A shim of tetrifact that tries to provide salient HTTP endpoints to work against.

## Requirements 

Python3. No other dependencies needed.

## Generating packages

The server will autogenerate random packages. You can regenerate packages by deleting `./v1/oackages.json` and restarting the server process.

If you want your packages to contain a specifc file add the file `./v1/.package` and add entries for each file you want to appear in your package, example

    - file : mything.exe
      content: this is some placeholder content
    - file : folder/otherfile.dat
      content: this is some more placeholder content

## How to

Run `sh ./start.sh` or `start.bat`.
