#!/usr/bin/env bash

sudo apt-get update

# dotnetcore
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-3.1 -y

# altecover report generator
dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.1.5

# utils
sudo apt install p7zip-full

# docker
sudo apt install docker.io -y
sudo apt install docker-compose -y
sudo usermod -aG docker vagrant

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "tetrifact" > /etc/hostname
sudo echo "127.0.0.1 tetrifact" >> /etc/hosts
