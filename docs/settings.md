## Tetrifact Settings

All Tetrifact settings are placed in a YML file that is read on start. The only exception to this is the path which Tetrifact uses to find this file - that path can be set with the env var `TETRIFACT_SETTINGS_PATH` which should point to some accessible, absolute path.

All YML settings are directly mapped to the class src/Tetrifact.Core/Settings.cs. All property names are capitalize, and must be printed exactly as they appear in that class. Default values are hardcoded in that class' constructure. Most properties are simple primatives, but there are collections that use standard YML notation.

    ServerName : My Server Name
    AllowPackageDelete : false
    ListPageSize : 100
    AccessTokens:
      - MyToken1
      - MyToken2
    TagColors:
      - Start: "MyTag:"  
      - Color: "#a1a1a1"

`ServerName` above is a string, `AllowPackageDelete` boolean, and `ListPageSize` an integer. `AccessTokens` is a list of strings, while `TagColors` is a collection of `TagColor` objects, each has two named string properties, `Start` and `Color`.


## 7zip

Tetrifact supports 7zip as a compression method for improved performance. Requires setting properies

    SevenZipBinaryPath : <path to 7za executable>
    DownloadArchiveMode : 7Zip

7za is the only part of 7zip that is required.

