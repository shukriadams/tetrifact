FROM ubuntu:20.04

# squelch tzdata prompt
ENV DEBIAN_FRONTEND=noninteractive

RUN apt-get update \
    && apt-get install -y wget \
    && apt-get install -y tzdata \
    && wget -q https://packages.microsoft.com/config/ubuntu/20.04/packages-microsoft-prod.deb \
    && sh -c 'dpkg -i packages-microsoft-prod.deb' \
    && apt-get install apt-transport-https -y \
    && apt-get update \
    && apt-get install aspnetcore-runtime-6.0 -y \
    && apt-get install p7zip-full -y \
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

# set Tetrifact default log level
ENV LOGGING__LOGLEVEL__DEFAULT=Information
ENV Logging__LogLevel__Microsoft=Warning
ENV SEVEN_ZIP_BINARY_PATH=/usr/lib/p7zip/7z

CMD sh -c 'cd /var/tetrifact && dotnet Tetrifact.Web.dll'
