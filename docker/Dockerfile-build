# this container is used for building. Build artefacts are copied out and passed to the
# hosting container image by the outer build script

FROM ubuntu:16.04

RUN apt-get update \
    && apt-get install -y wget \
    && wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb \
    && sh -c 'dpkg -i packages-microsoft-prod.deb' \
    && apt-get install apt-transport-https -y \
    && apt-get update \
    && apt-get install dotnet-sdk-2.2 -y \
    && mkdir -p /tmp/tetrifact

# keep container alive for building
CMD ["/bin/bash", "-c", "while true ; sleep 5; do continue ; done"]