# runs unit tests in tetrifact-build container

# kill any existing build container
docker-compose kill &&

docker-compose up -d &&

docker exec tetritest sh -c "cd /tmp/tetrifact && dotnet test"