#!/bin/bash
set -ex

###################
# AWS Deploy Script
###################

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )

main() {

    # Calls init() function which setups the binary for deployment
    set -o allexport
    source ./.env/aws.env
    source ./.env/local.env
    set +o allexport

    # Trading Variables
    declare -a strategies=("RandomStrategy") 
    declare -a symbolsArray=("EURUSD") #, "GBPUSD")
    stopLossInPipsRange="30 10 30"
    limitInPipsRange="30 10 30"
    iterationRange="1 1 1"

    # Remove binary files
    rm -rf ./src/bin/ ./src/obj/ ./tests/bin/ ./tests/obj/
    zip -r engine.zip ./src ./backtesting-engine.sln ./tests
    dotnet restore 

    # Associate the runID with the files, maybe move this to a commit ID in the future
    runID=$(uuidgen|sed -e 's/-//g')

    # Same this version to s3, to run the experiment below, runID is referenced in the script below
    aws s3api put-object --bucket ${awsDeployBucket} --key run/${runID}.zip --body ./engine.zip
    aws s3api put-object-tagging \
    --bucket ${awsDeployBucket}  \
    --key run/${runID}.zip \
    --tagging '{"TagSet": [{ "Key": "temp", "Value": "true" }]}'

    export reportingEnabled=true

    # Start the loop of the variables
    strategiesFunc

    # Remove the source files
    rm engine.zip
    echo $runID
}

strategiesFunc(){
    for strategy in "${strategies[@]}"
    do
        symbolsFunc
    done
}

symbolsFunc(){
    for symbols in "${symbolsArray[@]}"
    do
        iterationsFunc
    done
}

iterationsFunc(){
    # Run how many times
    for runIteration in `seq $iterationRange`
    do
        stopFunc
        limitFunc
        deploy
    done
}

stopFunc(){
    for pips in `seq $stopLossInPipsRange`
    do
        export stopDistanceInPips=$pips
    done
}

limitFunc(){
    for pips in `seq $limitInPipsRange`
    do
        export limitDistanceInPips=$pips
    done
}

deploy () {
    
    envsubst < $SCRIPT_DIR/awstemplate.sh > $SCRIPT_DIR/data.sh

    # Deploy
    aws ec2 run-instances   --image-id resolve:ssm:/aws/service/ami-amazon-linux-latest/amzn2-ami-hvm-arm64-gp2  \
                            --count 1 \
                            --instance-type ${awsDeployInstanceType//[-]/.} \
                            --key-name ${awsDeployKeyName} \
                            --subnet-id ${awsDeploySubnet} \
                            --security-group-ids ${awsDeploySecurityGroupID} \
                            --user-data file://${SCRIPT_DIR}/data.sh \
                            --iam-instance-profile=${awsDeployIAMEC2Role} \
                            --instance-initiated-shutdown-behavior terminate \
                            --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value='$runID'},{Key=Symbol,Value='$symbols'}]' >> /dev/null

    # Cleanup
    rm -rf $SCRIPT_DIR/data.sh
    sleep 0.8
}
main;
