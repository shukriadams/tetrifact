# get latest tag from current repo context
git fetch --tags &&
TAG=$(git describe --abbrev=0 --tags) &&

if [ ! -z $TAG ]; then
   echo "WARNING - project is not tagged";
   exit 1;
fi

# clone working copy of repo at the latest tag
rm -rf .clone &&
git clone --depth 1 --branch $TAG https://github.com/shukriadams/tetrifact.git .clone &&

rm -rf .artefacts &&
mkdir -p .artefacts &&

# kill any existing build container
docker-compose -f docker-compose-build.yml kill

# build and start new build container. the dotnetcore sdk takes up 1.7 gig of space
# so we want to build in a container, then copy the build artefacts out to the
# hosting container
docker-compose -f docker-compose-build.yml up -d

# write tag to currentVersion.txt in source, this will be displayed by web ui
echo $TAG > ./.clone/src/Tetrifact.Web/currentVersion.txt

# copy source code into build container and compile it.
docker cp ./.clone/src/. tetrifactbuild:/tmp/tetri &&
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet restore' &&
docker exec tetrifactbuild sh -c 'cd /tmp/tetrifact/Tetrifact.Web && dotnet publish /property:PublishWithAspNetCoreTargetManifest=false' &&
docker cp tetrifactbuild:/tmp/tetri/Tetri.Web/bin/Debug/netcoreapp2.2/publish/. ./.artefacts &&

# build hosting container
docker build -t shukriadams/tetrifact:$TAG .