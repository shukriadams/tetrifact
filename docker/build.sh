#! /bin/sh

# Use this script to manually build container locally. This script is NOT used by CI systems.

# fail on all errors
set -e

# get latest tag at current rev
TAG=$(git describe --abbrev=0 --tags)
if [ -z $TAG ]; then
   echo "Error, tag not set - please tag then rerun";
   exit 1;
fi

BUILD_CONTAINER=mcr.microsoft.com/dotnet/sdk:6.0

# copy source code into build container and compile it.
echo "Cleaning up"

rm -rf ./.artefacts

mkdir -p ./.artefacts
mkdir -p ./.tmp

# dotnet clean will not properly delete custom app data if present
# also, we delete via a container command because these files were created in a container, so, ownership
echo "manually cleaning up dev data if present"
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER rm -rf /tmp/tetrifact/Tetrifact.Tests/bin
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER rm -rf /tmp/tetrifact/Tetrifact.Tests/obj
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER rm -rf /tmp/tetrifact/Tetrifact.Web/bin
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER rm -rf /tmp/tetrifact/Tetrifact.Web/obj


echo "Copying src to tmp"
rsync -avP --exclude 'bin/*' --exclude 'obj/*' ./../src/. ./.tmp/.

# write tag to currentVersion.txt in source, this will be displayed by web ui
echo "Writing current version"
echo ${TAG} > ./.tmp/Tetrifact.Web/currentVersion.txt

# build it
echo "Building src"
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet restore' 
docker run -v "./.tmp:/tmp/tetrifact" $BUILD_CONTAINER sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false' 
cp -r ./.tmp/Tetrifact.Web/bin/Debug/net6.0/publish/. ./.artefacts 


# build hosting container
echo "Building deploy container"
docker build -t shukriadams/tetrifact . 

# test container, it should exit with 0 if successful
export TETRIFACT_SMOKETEST="true"
docker run -e TETRIFACT_SMOKETEST shukriadams/tetrifact:latest 

docker tag shukriadams/tetrifact:latest shukriadams/tetrifact:$TAG 

