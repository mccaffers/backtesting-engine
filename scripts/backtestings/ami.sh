###
### Script to setup AL2023 Image
###

## Install dependencies to unpack tick files and install .NET 8
sudo yum install -y zstd libicu

## Setup dotnet
wget -q https://dot.net/v1/dotnet-install.sh 
sh dotnet-install.sh -Channel 8.0
ln -s /root/.dotnet/dotnet /usr/bin/dotnet

## Free up some space in the image
sudo yum clean all -y
sudo find /var -name "*.log" \( \( -size +50M -mtime +7 \) -o -mtime +30 \) -exec truncate {} --size 0 \;
sudo rm -rf /var/cache/yum