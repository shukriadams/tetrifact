Param([parameter(Mandatory=$true, HelpMessage="Please enter a tag at which to build, example build.ps1 -tag 0.0.1")]
   $tag
)

mkdir out -ErrorAction SilentlyContinue

# clean clone the project at the target tag
Remove-Item -LiteralPath "clone" -Force -Recurse -ErrorAction SilentlyContinue
git clone --depth 1 --branch $tag https://github.com/shukriadams/tetrifact.git clone 

cd .\clone\src

# force remove existing publish folder if any
Remove-Item -LiteralPath ".\Tetrifact.Web\bin\Release\net6.0\publish" -Force -Recurse -ErrorAction SilentlyContinue

# run full dotnet publish
dotnet restore
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release

# zip publish target folder back up to folder from which we started

Compress-Archive -Force -Path .\Tetrifact.Web\bin\Release\net6.0\publish\* -DestinationPath .\..\..\out\Tetrifact.$($tag).zip

# return to whence we came
cd ..\..
