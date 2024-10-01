set -e
# tag must be passed in as an argument when calling this script
TAG=$1

if [ -z $TAG ]; then
   echo "Error, tag not set. Tag must be a valid github repo tag. Call this script : ./build myTag";
   exit 1;
fi

mkdir -p out

# clone working copy of repo at the latest tag
rm -rf clone 
git clone --depth 1 --branch $TAG https://github.com/shukriadams/tetrifact.git clone 

cd ./clone/src 

dotnet restore 
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release 

tar -czvf ./../../out/Tetrifact.$TAG.tar.gz -C ./Tetrifact.Web/bin/Release/net6.0/publish .

cd -