#!/bin/bash -x
{
export symbols=${symbols}
export runID=${runID}
export stopDistanceInPips=${stopDistanceInPips}
export limitDistanceInPips=${limitDistanceInPips}
export symbolFolder=${symbolFolder}
export scalingFactor="${scalingFactor}"
export accountEquity=${accountEquity}
export maximumDrawndownPercentage=${maximumDrawndownPercentage}
export strategy=${strategy}
export reportingEnabled=${reportingEnabled}
export s3Bucket="${s3Bucket}"
export s3Path=${s3Path}
export years=${years}
export elasticUser=${elasticUser}
export elasticPassword="${elasticPassword}"
export elasticEndpoint="${elasticEndpoint}"
export elasticCloudID="${elasticCloudID}"

export DOTNET_CLI_TELEMETRY_OPTOUT=1

# Pull the project files
sudo yum install -y libicu60 zstd
aws s3api get-object --bucket ${awsDeployBucket} --key run/${runID}.zip /home/ec2-user/files.zip &

# Setup dotnet
export HOME=/root/
wget https://dot.net/v1/dotnet-install.sh
sh dotnet-install.sh -c Current
ln -s /root/.dotnet/dotnet /usr/bin/dotnet

# Extract project files
cd /home/ec2-user/
mkdir /home/ec2-user/project
unzip files.zip -d ./project
rm -rf ./files.zip

# Run project
dotnet build /home/ec2-user/project -v q
dotnet run --project /home/ec2-user/project/src

poweroff
}  &> /home/ec2-user/output.txt 