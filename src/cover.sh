#! /bin/sh

# convenient single-click test + report batch file for local dev. Double-click in windows explorer to run.
rm -rf ./Tetrifact.Tests/coverage

dotnet test /p:AltCover=true

reportgenerator \
    -reports:./Tetrifact.Tests/coverage.xml \
    -targetdir:./Tetrifact.Tests/coverage \
    -assemblyfilters:"+Tetrifact.*;-Tetrifact.Tests;-Tetrifact.Web.Views;" \
    -classfilters:"-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*"

open Tetrifact.Tests/coverage/index.html