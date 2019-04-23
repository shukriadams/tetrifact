# tag must be passed in as an argument when calling this script
TAG=$1

if [ -z $TAG ]; then
   echo "Error, tag not set. Tag must be a valid github repo tag. Call this script : ./buildTag myTag";
   exit 1;
fi

# clone working copy of repo at the latest tag
rm -rf .clone &&
git clone --depth 1 --branch $TAG https://github.com/shukriadams/tetrifact.git .clone &&

rm -rf .artefacts &&
mkdir -p .artefacts &&

# kill any existing build container
docker-compose -f docker-compose-build.yml kill &&

# build and start new build container. the dotnetcore sdk takes up 1.7 gig of space
# so we want to compile the app in a private build container, then copy the artefacts out to the
# leaner hosting container
docker-compose -f docker-compose-build.yml up -d &&

# write tag to currentVersion.txt in source, this will be displayed by web ui
echo $TAG > ./.clone/src/Tetrifact.Web/currentVersion.txt &&

# copy source code into build container and compile it.
docker cp ./.clone/src/. tetrifactbuild:/tmp/tetrifact &&
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet restore' &&
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false' &&
docker cp tetrifactbuild:/tmp/tetrifact/Tetrifact.Web/bin/Debug/netcoreapp2.2/publish/. ./.artefacts &&

# build hosting container
docker build -t shukriadams/tetrifact . &&
docker tag shukriadams/tetrifact:latest shukriadams/tetrifact:$TAG 
