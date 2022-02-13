# convenient single-click test + report batch file for local dev. Double-click in windows explorer to run.

cd ./../src
# run test
dotnet test /p:AltCover=true
# generate cover
reportgenerator -reports:./Tetrifact.Tests/coverage.xml -targetdir:./coverage -assemblyfilters:"+Tetrifact.*;-Tetrifact.Tests" -classfilters:"-Tetrifact.Core.ThreadDefault"

