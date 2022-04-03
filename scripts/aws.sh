#!/bin/bash
set -ex

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

init() {
    
    set -o allexport
    source ./.env/aws.env
    source ./.env/local.env
    set +o allexport

    # Remove binary files
    rm -rf ./src/bin/ ./src/obj/ ./tests/bin/ ./tests/obj/
    zip -r engine.zip ./src ./backtesting-engine.sln ./tests
    dotnet restore 

    # Associate the runID with the files, maybe move this to a commit ID in the future
    runID=$(uuidgen|sed -e 's/-//g')

    # Same this version to s3, to run the experiment below, runID is referenced in the script below
    aws s3api put-object --bucket ${awsDeployBucket} --key run/${runID}.zip --body ./engine.zip
}

deploy () {
    
    envsubst < $SCRIPT_DIR/awstemplate.sh > $SCRIPT_DIR/data.sh

    # Deploy
    aws ec2 run-instances    --image-id resolve:ssm:/aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-arm64-gp2  \
                            --count 1 \
                            --instance-type ${awsDeployInstanceType//[-]/.} \
                            --key-name ${awsDeployKeyName} \
                            --subnet-id ${awsDeploySubnet} \
                            --security-group-ids ${awsDeploySecurityGroupID} \
                            --user-data file://${SCRIPT_DIR}/data.sh \
                            --iam-instance-profile=${awsDeployIAMEC2Role} \
                            --instance-initiated-shutdown-behavior terminate \
                            --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value='$runID'},{Key=Symbol,Value='$symbols'}]' >> /dev/null

    # # Cleanup
    rm -rf $SCRIPT_DIR/data.sh
}

###################
# AWS Deploy Script
###################

# Calls init() function which setups the binary for deployment
init

# For each strategy
#   For each symbol
#       Run how many times

export reportingEnabled=true

declare -a strategies=("RandomStrategy") 
for strategy in "${strategies[@]}"
do
    declare -a symbolsArray=("EURUSD") #, "GBPUSD")
    for symbol in "${symbolsArray[@]}"
    do
        for x  in `seq 1 1 1`
        do
            for pips  in `seq 20 10 20`
            do
                export stopDistanceInPips=$pips
                export limitDistanceInPips=$pips
                deploy
                sleep 0.8
            done
        done
    done
done

# Remove the source files
rm engine.zip
echo $runID