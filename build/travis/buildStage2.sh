# MUST BE RUN NATIVELY ON TRAVIS, ON NEW TAG
if [ ${#DOCKER_USER} -eq 0 ]; then
    echo "DOCKER_USER not set, exiting"
    exit 1;
fi

if [ ${#DOCKER_PASS} -eq 0 ]; then
    echo "DOCKER_PASS not set, exiting"
    exit 1;
fi

TAG=$(git describe --tags --abbrev=0)

rm -rf .stage &&
mkdir -p .stage &&
mkdir -p .stage/.artefacts &&

# stage docker file and artefacts
cp ./../../docker/Dockerfile ./.stage

# make local copy of build 
cp ./../../src/Tetrifact.Web/bin/Debug/netcoreapp2.2/publish/* ./.stage/.artefacts

# build hosting container
cd ./.stage
docker build -t shukriadams/tetrifact . &&
docker tag shukriadams/tetrifact:latest shukriadams/tetrifact:$TAG 
docker login -u $DOCKER_USER -p $DOCKER_PASS
docker push shukriadams/tetrifact:latest
docker push shukriadams/tetrifact:$TAG 

cd -