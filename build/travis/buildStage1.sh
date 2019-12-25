# THIS MUST BE RUN INSIDE BUILD CONTAINER, ON NEW TAG

# travis build, executed on tag. this script
# 1 - builds tetrifact binaries and pushes them to a public tetrifact server
# 2 - build docker image and pushes to hub.docker.com
if [ ${#TETRIFACT_UPLOAD_TOKEN} -eq 0 ]; then
    echo "TETRIFACT_UPLOAD_TOKEN not set, exiting"
    exit 1;
fi

cd /tmp/tetrifact/src

TAG=$(git describe --tags --abbrev=0)
if [ ${#TAG} -eq 0 ]; then
    echo "TAG not set, exiting"
    exit 1;
fi

# write tag to currentVersion.txt in source, this will be displayed by web ui
echo $TAG > ./Tetrifact.Web/currentVersion.txt &&

dotnet restore &&
dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release &&
cd ./Tetrifact.Web/bin/Release/netcoreapp3.1 &&
zip -r ./Tetrifact.$TAG.zip ./publish/*

curl -X POST \
    -H "Content-Type: multipart/form-data" \
    -H "Authorization: token ${TETRIFACT_UPLOAD_TOKEN}" \
    -F "Files=@./Tetrifact.${TAG}.zip;filename=Tetrifact.zip" \
    https://tetrifact.manafeed.com/v1/packages/Tetrifact.$TAG?isarchive=true


