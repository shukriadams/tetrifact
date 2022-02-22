:: convenient single-click test + report batch file for local dev. Double-click in windows explorer to run.

cd ./../src
:: run test
dotnet test /p:AltCover=true
:: generate cover
reportgenerator -reports:./Tetrifact.Tests/coverage.xml -targetdir:./coverage -assemblyfilters:+Tetrifact.*;-Tetrifact.Tests;-Tetrifact.Web.Views -classfilters:-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-*f__*
:: open cover report in browser
explorer "file:///%cd%/coverage/index.html"

