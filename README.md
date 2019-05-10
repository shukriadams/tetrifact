# Tetrifact

Master branch

[![Build Status](https://travis-ci.org/shukriadams/tetrifact.svg?branch=master)](https://travis-ci.org/shukriadams/tetrifact)

Develop branch

[![Build Status](https://travis-ci.org/shukriadams/tetrifact.svg?branch=develop)](https://travis-ci.org/shukriadams/tetrifact)

Tetrifact is a server that stores build arfefacts. It was written as a storage solution for continuous integration in the games industry, where frequent and large builds consume a lot of storage space and can be cumbersome to retrieve. Tetrifact exposes a simple HTTP REST API so it's easily integrated into your CI build chain. It also has a simple human-friendly interface.

Tetrifact is written in Dotnetcore 2.2, and should run on any system that supports Dotnetcore ASP. 

## Download

### As a Docker image

An Ubuntu-based version of Tetrifact is available as a Docker image @ https://hub.docker.com/r/shukriadams/tetrifact 

- Create a "data" directory in your intended Tetrifact deploy directory, Tetrifact will write all its files to this. 
- Tetrifact runs with user id 1000, and depending on your system might require permission to control this folder, set this with

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

### As binaries

Recent build :
[Version 1.0.0](https://github.com/shukriadams/tetrifact/releases/download/1.0.0/Tetrifact.1.0.0.zip)

Binaries for Tetrifact are available from Github under [releases](https://github.com/shukriadams/tetrifact/releases).

To start Tetrifact unzip and run

    dotnet Tetrifact.web.dll

Tetrifact expects all configuration to be passed in as environment variables - these can also be set from web.config.

## How it works

Tetrifact stores files on whatever filesystem it runs on. If a build contains a file with a unique hash, that file is written to disk. If another build contains a file with the same hash, it reuses the file from the first build. That is basically all that Tetrifact does. It's a file hash table running on a filesystem, with an HTTP API around it.

The built-in user interface for Tetrifact is rudimentary, and this is largely because one would construct a custom workflow on top of Tetrifact using its REST API. 

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
