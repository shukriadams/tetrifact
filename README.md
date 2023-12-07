# Tetrifact

Tetrifact is a server for storing build arfefacts. It is primarily written as a storage solution for continuous integration in the games industry, where frequent and large builds consume a lot of storage space and can be cumbersome to retrieve by automated process. 

### Features

- Exposes all artefacts and functionality via an HTTP REST API for easier integration with your CI and test chain 
- reduces storage space by sharing identical files across builds (file deduplication)
- handles millions of files, 100s of builds, 10s of gigabytes per builds - performs well on Unreal-scale projects with very large data footprints and file counts
- supports build hashing and comparison, allowing you to get file differences between two builds
- automatic pruning of old builds
- supports tagging of builds
- Written Docker- and Linux-first in C# on Dotnetcore 3.1, runs on any system that supports this framework

### Hardware

Tetrifact will run on pretty much any x86 hardware, but because it performs a lot of compression, hashing and JSON tasks, the more CPU cores it has the better. 

## Demo

A Tetrifact instance is running at *https://tetrifact.manafeed.com*. For internet reasons, all write operations from the web interface are disabled on this instance.

## How

Suppose you work for the ACME Game Corporation, and you're developing Thingernator. You've just commited your latest changes, and
your version control system says you're at revision 921. In your Thingernator build script, after compiling, zip Thingernator and then post it with

        curl \
            -X POST \ 
            -H "Content-Type: multipart/form-data" \
            -H "Transfer-Encoding: chunked" \
            -F "Files=@path/to/thingernator-build.zip" \
            http://tetriserver.example.com/v1/packages/Thingernator-921?isArchive=true 

Your QA team's automated test system wants builds of Thingernator. Tag your new build so it knows this build is testable.

        curl -X POST http://tetriserver.example.com/v1/tag/test-me!/Thingernator-921

The QA system can query new builds with

        curl http://tetriserver.example.com/v1/packages/latest/test-me! 
        -> returns "Thingernator-921"
        
or 

        curl http://tetriserver.example.com/v1/tags/test-me!/packages 
        -> returns ["Thingernator-921"], a JSON array of builds with "Test-me!" tag.

A zip of the build can then be downloaded with
        
        curl http://tetriserver.example.com/v1/archives/Thingernator-921

## Download 

### Binaries

Binary builds require DotNetCore 3.1 or better to run. Binaries can be downloaded from the 
[official Tetrifact demo server](https://tetrifact.manafeed.com).

To start Tetrifact unzip and from the command line run

    dotnet Tetrifact.web.dll

All configuration is passed in as environment variables - these can also be set from the web.config file.

### Docker image

A Linux version of Tetrifact is available via Docker @ https://hub.docker.com/r/shukriadams/tetrifact 

- Create a "data" directory in your intended Tetrifact deploy directory, Tetrifact will write all its files to this. 
- Tetrifact runs with user id 1000, and needs permission to control this folder, set this with

        chown -R 1000 ./data

- Assuming you are starting with docker-compose, use the following example config and customize as needed

        version: "2"
        services:
        tetrifact:
            image: shukriadams/tetrifact:<TAG>
            container_name: tetrifact
            restart: unless-stopped
            environment:
              ASPNETCORE_URLS : http://*:5000
            volumes:
              - ./data:/var/tetrifact/data/:rw
            ports:
            - "49022:5000"

Note that Docker for Windows now supports Linux containers, so you can run this container on Windows hosts too. 

## What it isn't

Tetrifact is use-at-your-own risk open source software. It is intended for use in your in-house CI build chain, and replaces the awful practice of storing builds on SMB file servers. Tetrifact is not a version control system or bullet-proof archive. It's written to be robust and fault-tolerant in a real-life game studio with multiple large daily builds, but you should still probably not use it for absolutely irreplacable files such as release-to-manufacture builds. 

## Using

See the [advanced use docs](/docs/use.md) for how to use Tetrifact.

## Development

See the [developer docs](/docs/development.md) for details on running Tetrifact in a development enviroment if you're interested in debugging or contributing.

## Security

Tetrifact has zero security - it is 100% open and intended for use on internal networks where everyone is trusted. _Never_ expose your Tetrifact instance to "the web" unless you know what you're doing. 

## License

MIT (see license file for more information)
 
