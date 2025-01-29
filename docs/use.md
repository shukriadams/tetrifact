# Using Tetrifact

Tetrifact stores builds as packages. A package is a group of files which are added and retrieved as a unit, and should correspond to a single build in your CI system, which in turn should of course correspond to a COMMIT or REVISION in your VCS (version control system). It is highly recommended you name your Tetrifact packages after the revision number (or hash) in your VCS the package's contents were compiled from, as this allows you to easily trace a package back to the source code from which it was built.

Don't worry if your revision hashes are difficult to pass around, share or remember, packages can be tagged with additional human-friendly names too.

## Changing log level

The default log level as of version 1.8.4 is `Warning`. You can override this with the environment variable

    Logging__LogLevel__Microsoft=<YOUR LEVEL>

Allowed levels are the standard internal Dotnet values : Trace|Debug|Information|Warning|Error|Critical|None

## REST

All Tetrifact's functionality is exposed via a REST API, so it should be familiar.

## Adding a package

### As individual files

HTTP METHOD :

    POST

ENDPOINT :

    /v1/packages/myPackageName

HEADER :

    Content-Type: multipart/form-data; boundary=-------------------------acebdf13572468 

BODY :

    ---------------------------acebdf13572468
    Content-Disposition: form-data; name="Files"; filename="1.txt" 

    file 1 text or binary content here
    ---------------------------acebdf13572468
    Content-Disposition: form-data; name="Files" ; filename="path/to/2.txt"

    file 2 text or binary content here
    ---------------------------acebdf13572468

Each file's "name" property must be "Files". The filename property should be the path and filename of the file you want to store, relative to the root of the package.

Were you posting actual files with CURL it should look like

    curl -X POST \
        -H "Content-Type: multipart/form-data" \
        -H "Transfer-Encoding: chunked" \
        -F "Files=@~/mybuild/1.txt;filename=1.txt" \
        -F "Files=@~/mybuild/path/to/2.txt;filename=path/to/2.txt" \
        http://myTetrifact.server/v1/packages/myPackageName 

### As an archive

To post your build as an archive, use the following.

HTTP METHOD :

    POST

ENDPOINT :

    /v1/packages/myPackageName?IsArchive=true

HEADER :

    Content-Type: multipart/form-data; boundary=-------------------------acebdf13572468 

BODY :

    ---------------------------acebdf13572468
    Content-Disposition: form-data; name="FILES"; filename="whatever"
    Content-Type: application/octet-stream

    <@INCLUDE *~/path/to/archive.zip*@>
    ---------------------------acebdf13572468

When posting a zip, note the following

- only a single file can be attached to your post. If you add more than one, you'll get an error.
- filename doesn't matter, as it won't be used
- the archive's root will be treated as the root of the project, meaning all file paths will be mapped relative to this.

With CURL it would look like

    curl -X POST -H "Content-Type: multipart/form-data" -H "Transfer-Encoding: chunked" -F "Files=@path/to/archive" http://tetriserver.example.com/v1/packages/myPackage?isArchive=true 

Chunking the upload is important if your archive is large, as this might exceeded the multipart body attachment size.

### Posting an archive from NodeJS

This uses the request package (https://www.npmjs.com/package/request).

    let request = require('request'),
        fs = require('fs'),
        formdata = {
            Files : fs.createReadStream('path/to/archive')
        };

    request.post({url: 'http://tetriserver.example.com', formData: formdata}, function(err, httpResponse, body) {
        if (err) {
            return console.error('upload failed : ', err);
        }

        console.log('Upload succeeded : ', body);
    });

## Tagging

### Curl

To add the tag "MyTag" to the package "MyPackage, use

    curl -X POST http://tetriserver.example.com/v1/tags/MyTag/MyPackage

To remove the tag

    curl -X DELETE http://tetriserver.example.com/v1/tags/MyTag/MyPackage


## Pruning

Tetrifact has automated pruning to delete older packages to prevent disk overuse. All prune settings are kept in config.yml.

Pruning is disabled by default, to enable it use

    ...
    PruneEnabled : True
    ...

Pruning behaviour works in brackets. A bracket is a period for the age of packages, and x number of packages of a given age can be kept. Using the example below

    PruneBrackets :
    -   Days: 1
        Amount: -1
    -   Days : 3
        Amount : 5
    -   Days : 5
        Amount : 4

The first bracket is for all packages up to 1 day old, an amount of -1 means prune none, ie, keep all packages that are up to a day old. The second bracket indicates that for all packages created more than 1 ago and less than 3 days ago, keep 5 packages.

Pruning can ignore packages marked with one or more tags, use the list 

    PruneIgnoreTags:
    -   MyKeepTag
    -   MyImportTag

Pruning runs at 2am server time by default, you can change this with

    PruneCronMask: "0 3 * * *"


