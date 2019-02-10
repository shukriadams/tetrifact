# Tetrifact

Tetrifact is a server that stores build arfefacts. It was written as a storage solution for continuous integration in the games industry, where frequent and large builds consume a lot of storage space and can be cumbersome to retrieve. Tetrifact exposes a simple HTTP REST API so it's easily integrated into your CI build or deployment chain. It also has a user interface so users can retrieve builds directly from it.

Tetrifact is written in Dotnetcore, and should run on any system that supports Dotnetcore ASP. It is available as a Docker image at https://cloud.docker.com/repository/docker/shukriadams/tetrifact


## How it works

Tetrifact stores files to whatever filesystem it runs on. If a build contains a file with a unique hash, that file is written to the file system. If another build contains a file with the same hash, it reuses the file from the first build. That is basically all that Tetrifact does. It's a file hash table running on a filesystem, with an HTTP API around it. 


## What it isn't

Tetrifact is intended for use in your in-house CI build chain, and replaces the awful practice of storing builds in static folders on file servers. Tetrifact is not a super bullet-proof file-database-engine adhering to ACID principles - it's written to be robust and fault-tolerant, but you should probably not use it to store irreplacable files. Think of it like Memcached - it makes things faster and easier, and you could lose everything in it from time to time. 


## Using

See /docs/USE.md for documention on how to use Tetrifact.


## Development

See /docs/DEVELOPMENT.md for details if you're interested in contributing to Tetrifact's development.


## Security

Tetrifact's current has no security model - it is 100% open. Use it on an internal network where everyone is trusted. 


## License

MIT (see license file for more information)