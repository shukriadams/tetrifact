#!/usr/bin/env bash

sudo apt-get update

# dotnetcore
wget -q https://packages.microsoft.com/config/ubuntu/18.04/packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
sudo apt-get install apt-transport-https
sudo apt-get update
sudo apt-get install dotnet-sdk-6.0 -y

# altecover report generator
dotnet tool install --global dotnet-reportgenerator-globaltool --version 4.1.5

# docker
sudo mkdir -p /usr/libexec/docker/cli-plugins
sudo apt install docker.io -y
sudo usermod -aG docker vagrant
sudo wget https://github.com/docker/compose/releases/download/v2.29.1/docker-compose-linux-x86_64 -O /usr/libexec/docker/cli-plugins/docker-compose
sudo chmod +x /usr/libexec/docker/cli-plugins/docker-compose
echo "export PATH=/usr/libexec/docker/cli-plugins:$PATH" >> /home/vagrant/.bashrc

# force startup folder to vagrant project
echo "cd /vagrant/src" >> /home/vagrant/.bashrc

# set hostname, makes console easier to identify
sudo echo "tetrifact" > /etc/hostname
sudo echo "127.0.0.1 tetrifact" >> /etc/hosts