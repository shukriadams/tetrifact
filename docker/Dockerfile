FROM ubuntu:18.04

RUN apt-get update \
    && apt-get install -y wget \
    && wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb \
    && sh -c 'dpkg -i packages-microsoft-prod.deb' \
    && apt-get install apt-transport-https -y \
    && apt-get update \
    && apt-get install aspnetcore-runtime-3.1 -y \
    ## clean up
    && rm packages-microsoft-prod.deb \
    && apt-get remove wget -y \
    && apt-get remove apt-transport-https -y \
    && useradd -u 1000 tetrifact \
    && mkdir -p /var/tetrifact \
    && chmod 700 -R /var/tetrifact \
    && chown tetrifact /var/tetrifact

COPY ./.artefacts/. /var/tetrifact

USER tetrifact

CMD sh -c 'cd /var/tetrifact && dotnet Tetrifact.Web.dll'
