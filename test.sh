# kill any existing build container
docker-compose kill &&

docker-compose up -docker &&

docker exec tetrifactest sh -c "cd /tmp/tetrifact && dotnet test"