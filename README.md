# Tetrifact


## Status

Master branch

[![Build Status](https://travis-ci.org/shukriadams/tetrifact.svg?branch=master)](https://travis-ci.org/shukriadams/tetrifact)

Develop branch

[![Build Status](https://travis-ci.org/shukriadams/tetrifact.svg?branch=develop)](https://travis-ci.org/shukriadams/tetrifact)

Tetrifact is a server that stores build arfefacts. It was written as a storage solution for continuous integration in the games industry, where frequent and large builds consume a lot of storage space and can be cumbersome to retrieve by automated process. Tetrifact cuts down on storage space by sharing identical files across builds. It exposes an HTTP REST API so it can easily be integrated into your CI build chain. It also has a simple human-friendly interface.

It is written in Dotnetcore 2.2, and will run on any system that supports this framework.

## How

Lets suppose you work for the ACME Game Corporation, and you're developing Thingernator.
In your Thingernator build script, after compiling, zip Thingernator and then post it with

        curl 
            -X POST 
            -H "Content-Type: multipart/form-data" 
            -F "Files=@path/to/thingernator-build.zip" 
            http://tetriserver.example.com/v1/packages/Thingernator-alpha-build-0.0.6?isArchive=true 

Your QA team's automated test system wants builds of Thingernator. Tag your new build so it know this build is testable.

        curl -X POST http://tetriserver.example.com/v1/tag/test-me!/Thingernator-alpha-build-0.0.6

The QA system can query new builds with

        curl http://tetriserver.example.com/v1/packages/latest/test-me! 
        -> returns "Thingernator-alpha-build-0.0.6"
        
or 

        curl http://tetriserver.example.com/v1/tags/test-me!/packages 
        -> returns a JSON array of builds with "Test-me!" tag.

A zip of the build can then be downloaded with
        
        curl http://tetriserver.example.com/v1/archives/Thingernator-alpha-build-0.0.6
        
## Demo

Tetrifact is now self-hosting - you can download builds of Tetrifact from a Tetrifact instance *https://tetrifact.manafeed.com*, which also acts as a convenient demo of the server interface. Note that all write/change operations on this instance are disabled.

## Download 

### Binaries

Binary builds require DotNetCore 2.2 or better to run. Binaries can be found under [releases](https://github.com/shukriadams/tetrifact/releases), or from the  
[Tetrifact demo server](https://hub.docker.com/r/shukriadams/tetrifact).

To start Tetrifact unzip and from the command line run

    dotnet Tetrifact.web.dll

All configuration is passed in as environment variables - these can also be set from web.config.

### Docker image

A Linux version of Tetrifact is available via Docker @ https://hub.docker.com/r/shukriadams/tetrifact 

- Create a "data" directory in your intended Tetrifact deploy directory, Tetrifact will write all its files to this. 
- Tetrifact runs with user id 1000, and needs permission to control this folder, set this with

        chown -R 1000 ./data

- Assuming you are starting with docker-compose, use the following example config and customize as needed

        version: "2"
        services:
        tetrifact:
            image: shukriadams/tetrifact:latest
            container_name: tetrifact
            restart: unless-stopped
            environment:
              ASPNETCORE_URLS : http://*:5000
            volumes:
              - ./data:/var/tetrifact/data/:rw
            ports:
            - "49022:5000"

## What it isn't

Tetrifact is intended for use in your in-house CI build chain, and replaces the awful practice of storing builds in static folders on file servers. Tetrifact is not a super bullet-proof file-database-engine-thinger that adheres to ACID principles - it's written to be robust and fault-tolerant, in a real-life game studio with multiple daily builds, but you should still probably not use it to store absolutely irreplacable files. 

## Using

See /docs/use.md for documention on how to use Tetrifact.

## Development

See /docs/development.md for details if you're interested in contributing to Tetrifact's development.

## Security

NOTE : Tetrifact currently has no security model - it is 100% open. Use it on an internal network where everyone is trusted. Security will be added later.

## License

MIT (see license file for more information)
