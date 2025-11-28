:: Convenient single-click script to run cover. Works on Windows only.

dotnet test /p:AltCover=true

reportgenerator -reports:./Tetrifact.Tests/coverage.xml -targetdir:./Tetrifact.Tests/coverage -assemblyfilters:+Tetrifact.*;-Tetrifact.Tests -classfilters:-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*

start Tetrifact.Tests\coverage\index.html