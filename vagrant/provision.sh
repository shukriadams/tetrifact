#!/usr/bin/env bash

sudo apt-get update

# dotnetcore
wget -q https://packages.microsoft.com/config/ubuntu/16.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-2.2 -y

# docker
sudo apt install docker.io -y
sudo apt install docker-compose -y
sudo usermod -aG docker vagrant

# force github into known hosts so build script can clone without prompt. yes, this is 
# insecure because it opens for MITM attack, but I don't have anything better right now.
ssh-keyscan github.com >> ~/.ssh/known_hosts

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "tetri" > /etc/hostname
sudo echo "127.0.0.1 tetri" >> /etc/hosts
