# runs unit tests in tetrifact-build container

# kill any existing build container
docker-compose kill &&

# start container from docker-compose file in this folder; this is for the tetrifact-build container
# (see https://github.com/shukriadams/tetrifact-build for details)
docker-compose up -d &&

# runs Tetrifact.Tests project in build container
docker exec tetrifactbuild sh -c "cd /tmp/tetrifact/src && dotnet test /p:AltCover=true"
