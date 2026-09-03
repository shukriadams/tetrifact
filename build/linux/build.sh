set -e


TAG=$(git describe --abbrev=0 --tags)

if [ -z $TAG ]; then
   echo "Error, tag not set - please tag then rerun";
   exit 1;
fi

cd ./../../src

dotnet restore 
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release 

cd -