# Use this script to manually build container locally. This script is NOT used by CI systems.

# fail on all errors
set -e

# get latest tag at current rev
TAG=$(git describe --abbrev=0 --tags)
if [ -z $TAG ]; then
   echo "Error, tag not set - please tag then rerun";
   exit 1;
fi

rm -rf .artefacts 
mkdir -p .artefacts



# kill any existing build container
docker compose -f docker-compose-build.yml kill 

# build and start new build container. the dotnetcore sdk takes up 1.7 gig of space
# so we want to compile the app in a private build container, then copy the artefacts out to the
# leaner hosting container
docker compose -f docker-compose-build.yml up -d 


# copy source code into build container and compile it.
docker cp ./../src/. tetrifactbuild:/tmp/tetrifact 


# dotnet clean will not properly delete custom app data, remove manually
docker exec tetrifactbuild sh -c "rm -rf /tmp/tetrifact/Tetrifact.Tests/bin"
docker exec tetrifactbuild sh -c "rm -rf /tmp/tetrifact/Tetrifact.Tests/obj"
docker exec tetrifactbuild sh -c "rm -rf /tmp/tetrifact/Tetrifact.Web/bin"
docker exec tetrifactbuild sh -c "rm -rf /tmp/tetrifact/Tetrifact.Web/obj"

# write tag to currentVersion.txt in source, this will be displayed by web ui
docker exec tetrifactbuild sh -c "echo ${TAG} > /tmp/tetrifact/Tetrifact.Web/currentVersion.txt"

# build it
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet restore' 
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false' 
docker cp tetrifactbuild:/tmp/tetrifact/Tetrifact.Web/bin/Debug/netcoreapp3.1/publish/. ./.artefacts 

# kill build container
docker compose -f docker-compose-build.yml kill 

# build hosting container
docker build -t shukriadams/tetrifact . 
docker tag shukriadams/tetrifact:latest shukriadams/tetrifact:$TAG 
