## Tetrifact Settings

Most Tetrifact settings are stored in a YML file that is read on start. This file must be placed in the application binary directory, and the default expected name is `config.yml`. 

All settings in the main config file are directly mapped to the class src/Tetrifact.Core/Settings.cs. All property names are capitalize, and must be printed exactly as they appear in that class. Default values are hardcoded in that class' constructure. Most properties are simple primatives, but there are collections that use standard YML notation.

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

### Environment variable settings

A few settings are only accessible as environment variables. These are typically values that are needed before YML config can be loaded, or which are tied to the underlying Dotnet framework and are not easy to override from code. These settings are :

#### config.yml path

- `TETRIFACT_SETTINGS_PATH` : Overrides the default config.yml path of `<application-binary-root>/config.yml`. Absolute path expected. On start, Tetrifact confirms which path config is loaded from.

- `LOGGING__LOGLEVEL__DEFAULT` : Default value is `Information` for Docker builds. Sets log level. Allowed values are the standard Dotnet log level enum values in string form, namely `Trace|Debug|Information|Warning|Error|Critical|None`. 

- `LOGGING__LOGLEVEL__Microsoft` : Default is `Warning` for Docker builds. Sets log level for Microsoft-namespaced server components, which normally flood your logs with noise under regular use.

### Details

A detailed list of settings are :

(TBD)

#### 7zip

Tetrifact supports 7zip as a compression method for improved performance. Requires setting properies

    SevenZipBinaryPath : <path to 7za executable>
    DownloadArchiveMode : 7Zip

7za is the only part of 7zip that is required.

