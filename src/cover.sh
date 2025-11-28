# Convenient single-click script to run cover. 

dotnet test /p:AltCover=true

reportgenerator \
    -reports:./Tetrifact.Tests/coverage.xml \
    -targetdir:./Tetrifact.Tests/coverage \
    -assemblyfilters:"+Tetrifact.*;-Tetrifact.Tests" \
    -classfilters:"-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*"

open Tetrifact.Tests/coverage/index.html