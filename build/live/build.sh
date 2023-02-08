set -e
DOCKERPUSH=0
SMOKETEST=0

while [ -n "$1" ]; do 
    case "$1" in
    --dockerpush|-p) DOCKERPUSH=1 ;;
    --smoketest|-t) SMOKETEST=1 ;;
    esac 
    shift
done

TAG=$(git describe --tags --abbrev=0)
if [ -z "$TAG" ]; then
    echo "TAG not set, exiting"
    exit 1;
fi

# write tag to currentVersion.txt in source, this will be displayed by web ui
echo $TAG > ./../../src/Tetrifact.Web/currentVersion.txt 

docker run \
    -e TAG=$TAG \
    -v $(pwd)./../../src:/tmp/tetrifact \
    shukriadams/tetrifact-build:0.0.4 \
    sh -c "cd /tmp/tetrifact && \
        dotnet restore && \
        dotnet publish /property:PublishWithAspNetCoreTargetManifest=false --configuration Release"

cd ./../../src/Tetrifact.Web/bin/Release/netcoreapp3.1 
zip -r ./Tetrifact.$TAG.zip ./publish/*

rm -rf .stage 
mkdir -p .stage 
mkdir -p .stage/.artefacts 

cd -

# stage docker file and artefacts
cp ./../../docker/Dockerfile ./.stage

# make local copy of build 
cp -R ./../../src/Tetrifact.Web/bin/Release/netcoreapp3.1/publish/* ./.stage/.artefacts

# build hosting container
cd ./.stage
docker build -t shukriadams/tetrifact . 
docker tag shukriadams/tetrifact:latest shukriadams/tetrifact:$TAG 
cd ..

if [ $SMOKETEST -eq 1 ]; then
    docker-compose -f docker-compose-test.yml down 
    docker-compose -f docker-compose-test.yml up -d 
    sleep 5  # wait a few seconds to make sure app in container has started
    STATUS=$(curl -s -o /dev/null -w "%{http_code}" localhost:49022) 
    docker-compose -f docker-compose-test.yml down 
    if [ "$STATUS" != "200" ]; then
        echo "test container returned unexpected value ${STATUS}"
        exit 1
    else
        echo "smoke test passed"
    fi
fi

if [ $DOCKERPUSH -eq 1 ]; then
    docker login -u $DOCKER_USER -p $DOCKER_PASS 
    docker push shukriadams/tetrifact:$TAG  
fi

if [ ! -z $TETRIFACT_UPLOAD_TOKEN ]; then
    curl -X POST \
        -H "Content-Type: multipart/form-data" \
        -H "Authorization: token ${TETRIFACT_UPLOAD_TOKEN}" \
        -F "Files=@./../../src/Tetrifact.Web/bin/Release/netcoreapp3.1/Tetrifact.${TAG}.zip;filename=Tetrifact.zip" \
        https://tetrifact.manafeed.com/v1/packages/Tetrifact.$TAG?isarchive=true
fi

cd -