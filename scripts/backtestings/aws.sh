#!/bin/bash
set -ex

###################
# AWS Deploy Script
###################

SCRIPT_DIR=$( cd -- "$( dirname -- "${BASH_SOURCE[0]}" )" &> /dev/null && pwd )
AWS_MACHINE_IMAGE="resolve:ssm:/aws/service/ami-amazon-linux-latest/al2023-ami-kernel-6.1-arm64"

main() {

    # Calls init() function which setups the binary for deployment
    set -o allexport
    source ./.env/aws.env
    source ./.env/local.env
    set +o allexport

    my_dir=$( dirname "${BASH_SOURCE[0]}" )
    echo $my_dir
    
    source $my_dir/variables/defaults

    ## Trailing Stop Loss
    # kineticStopLossRange="0 1 0"
    # kineticLimitRange="0 1 0"
    
    ## Random HH LL Strategy Customs
    # randomStrategyAmountOfHHLLSseq="5 5 5"

    # source $my_dir/experiments/random/forex/fixed
    source $my_dir/experiments/random/forex/variable
    ######

    # Remove binary files & tests
    find . -iname "bin" -o -iname "obj" | xargs rm -rf
    zip -r engine.zip ./src ./backtesting-engine.sln ./tests -x "src/ui/*" -y
    dotnet restore 

    # Associate the runID with the files, maybe move this to a commit ID in the future
    runID=$(uuidgen|sed -e 's/-//g')

    # Same this version to s3, to run the experiment below, runID is referenced in the script below
    aws s3api put-object --bucket ${awsDeployBucket} --key run/${runID}.zip --body ./engine.zip
    aws s3api put-object-tagging \
    --bucket ${awsDeployBucket}  \
    --key run/${runID}.zip \
    --tagging '{"TagSet": [{ "Key": "temp", "Value": "true" }]}'

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
    done
}

stopFunc(){
    for stopLossVaue in `seq $stopLossInPipsRange`
    do
        export stopDistanceInPips=$stopLossVaue
        limitFunc
    done
}

limitFunc(){
    for limitValue in `seq $limitInPipsRange`
    do
        export limitDistanceInPips=$limitValue
        deploy
    done
}

kineticSLFunc(){
    for pips in `seq $kineticStopLossRange`
    do
        export kineticStopLoss=$pips
        kineticTPFunc
    done
}

kineticTPFunc(){
    for pips in `seq $kineticLimitRange`
    do
        export kineticLimit=$pips
        
        randomStrategyHHLLFunc
    done
}

randomStrategyHHLLFunc(){
    for pips in `seq $randomStrategyAmountOfHHLLSseq`
    do
        export randomStrategyAmountOfHHLL=$pips
        deploy
    done
}


instanceCount=0

deploy () {

    let instanceCount=instanceCount+1
    
    envsubst < $SCRIPT_DIR/template.txt > $SCRIPT_DIR/data.sh

    # Deploy
    aws ec2 run-instances   --image-id ${AWS_MACHINE_IMAGE}  \
                            --count 1 \
                            --instance-type ${awsDeployInstanceType//[-]/.} \
                            --key-name ${awsDeployKeyName} \
                            --subnet-id ${awsDeploySubnet} \
                            --security-group-ids ${awsDeploySecurityGroupID} \
                            --iam-instance-profile=${awsDeployIAMEC2Role} \
                            --user-data file://${SCRIPT_DIR}/data.sh \
                            --instance-initiated-shutdown-behavior terminate \
                            --block-device-mapping "[ { \"DeviceName\": \"/dev/xvda\", \"Ebs\": { \"VolumeSize\": 12 } } ]" \
                            --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value='$runID'},{Key=Symbol,Value='$symbols'}]' --no-cli-pager 

    # Cleanup
    rm -rf $SCRIPT_DIR/data.sh
    sleep 0.6
}
main;
