:: Convenient single-click script to run cover. Works on Windows only.

rmdir ./Tetrifact.Tests/coverage /s /q

:: run test
dotnet test /p:AltCover=true

:: generate cover
reportgenerator ^
    -reports:./Tetrifact.Tests/coverage.xml ^
    -targetdir:./Tetrifact.Tests/coverage ^
    -assemblyfilters:"+Tetrifact.*;-Tetrifact.Tests;-Tetrifact.Web.Views" ^
    -classfilters:"-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*"

:: open cover report in browser
start "file:///%cd%/Tetrifact.Tests/coverage/index.html"

pause
