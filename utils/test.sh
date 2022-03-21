
# convenient single-click test + report batch file for local dev. Double-click in windows explorer to run.
set -e

cd ./../src

# run test
dotnet test /p:AltCover=true
exit
# generate cover
reportgenerator -reports:./Tetrifact.Tests/coverage.xml -targetdir:./coverage \
    -assemblyfilters:+Tetrifact.*;-Tetrifact.Tests;-Tetrifact.Web.Views \
    -classfilters:-Tetrifact.Core.ThreadDefault;-Tetrifact.Web.DaemonProcessRunner;-Tetrifact.Web.Pager;-Tetrifact.Web.Program;-Tetrifact.Web.Startup;-Tetrifact.Web.ReadLevel;-Tetrifact.Web.WriteLevel;-*f__*



