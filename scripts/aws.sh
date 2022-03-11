#!/bin/bash
set -ex

# Remove binary files
rm -rf ./src/bin/
rm -rf ./src/obj/
rm -rf ./tests/bin/
rm -rf ./tests/obj/
zip -r engine.zip ./src ./backtesting-engine.sln ./tests
dotnet restore 

# Associate the runID with the files, maybe move this to a commit ID in the future
runID=$(uuidgen|sed -e 's/-//g')

# Same this version to s3, to run the experiment below, runID is referenced in the script below
aws s3api put-object --bucket ${awsDeployBucket//[-]/.} --key run/${runID}.zip --body ./engine.zip

deploy () {

rm -rf  ./data.sh
cat << EOF >> ./data.sh
#!/bin/bash -x
{
EOF

while read line; do echo "export "$line  >> ./data.sh; done < .env/local.env

# cat ./.env/local.env >> ./data.sh

cat << EOF >> ./data.sh
# Pull the project files
aws s3api get-object --bucket ${awsDeployBucket//[-]/.} --key run/${runID}.zip /home/ec2-user/files.zip &

# Setup dotnet
export HOME=/root/
wget https://dot.net/v1/dotnet-install.sh
sh dotnet-install.sh -c Current
ln -s /root/.dotnet/dotnet /usr/bin/dotnet
sudo yum install -y libicu60

# Extract project files
cd /home/ec2-user/
mkdir /home/ec2-user/project
unzip files.zip -d ./project
rm -rf ./files.zip

# Run project
dotnet build /home/ec2-user/project
dotnet run --project  /home/ec2-user/project/src

poweroff
}  &> /home/ec2-user/output.txt 
EOF

# Deploy
aws ec2 run-instances    --image-id resolve:ssm:/aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-arm64-gp2  \
                         --count 1 \
                         --instance-type ${awsDeployInstanceType//[-]/.} \
                         --key-name ${awsDeployKeyName} \
                         --subnet-id ${awsDeploySubnet} \
                         --security-group-ids ${awsDeploySecurityGroupID} \
                         --user-data file://data.sh \
                         --iam-instance-profile=${awsDeployIAMEC2Role} \
                         --instance-initiated-shutdown-behavior terminate;

# Cleanup
rm data.sh
rm engine.zip
}

# For each strategy
#   For each symbol
#       Run how many times
declare -a strategies=("RandomTrade") 
for strategy in "${strategies[@]}"
do
    declare -a symbols=("EURUSD")
    for symbol in "${symbols[@]}"
    do
        for x  in `seq 1 1 1`
        do
            deploy
        done
    done
done


