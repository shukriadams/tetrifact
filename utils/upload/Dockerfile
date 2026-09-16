FROM ubuntu:24.04

# squelch tzdata prompt
ENV DEBIAN_FRONTEND=noninteractive

RUN apt update \
    && apt install curl -y \
    && apt install python3-minimal -y \
    && apt install python3-pip -y \
    && mkdir -p /scripts 

COPY ./. /scripts/
