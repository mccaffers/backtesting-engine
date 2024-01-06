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

    ######
    # Trading Variables

    # declare -a strategies=("VolatilityCalculator") 
    declare -a strategies=("RandomStrategy") 
    # declare -a strategies=("RandomWithCloseAtHHLL") 

    # Major Forex Currencies
    declare -a symbolsArray=("EURUSD" "USDJPY" "GBPUSD" "NZDUSD" "USDCHF" "USDCAD" "AUDUSD")
    yearsStart=2004

    ## Multiple Indexes
    # declare -a symbolsArray=("JPNIDX225" "SPNIDX35" "FRAIDX40" "DEUIDX40" "AUSIDX200" "USAIDXTECH" "USAIDX500" "USAIDX30" "EURIDX600" "GBRIDX100")
    # yearsStart="2014"
    
    ## Crypto
    # declare -a symbolsArray=("ETHUSD" "BTCUSD")
    # yearsStart="2018"

    #####
    # Trading Defaults
    # Do not change here
    stopLossInPipsRange="1 1 1"
    limitInPipsRange="1 1 1"
    iterationRange="1 1 2"
    yearsEnd=2021
    accountEquity=10000
    maximumDrawndownPercentage=75
    kineticOn=0 # 1 on, 0 off
    kineticStopLossRange="0 1 0"
    kineticLimitRange="0 1 0"
    randomStrategyAmountOfHHLLSseq="0 5 0"
    #######

    ####
    # Customisable Trading Variables

    # STOP LOSS Distance in PIPs
    stopLossInPipsRange="30 1 30"
    
    # TAKE PROFIT Distance in PIPs
    limitInPipsRange="30 1 30"
    
    # Account Equity
    accountEquity=0
    maximumDrawndownPercentage=0
    yearsEnd=2023
    # maximumDrawndownPercentage=75
    
    ## Trailing Stop Loss
    # kineticOn=0 # 1 on, 0 off
    # kineticStopLossRange="0 1 0"
    # kineticLimitRange="0 1 0"
    
    ## Random HH LL Strategy Customs
    # randomStrategyAmountOfHHLLSseq="5 5 5"

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
                            --user-data file://${SCRIPT_DIR}/data.sh \
                            --iam-instance-profile=${awsDeployIAMEC2Role} \
                            #  --instance-initiated-shutdown-behavior terminate \
                            --block-device-mapping "[ { \"DeviceName\": \"/dev/xvda\", \"Ebs\": { \"VolumeSize\": 12 } } ]" \
                            --tag-specifications 'ResourceType=instance,Tags=[{Key=Name,Value='$runID'},{Key=Symbol,Value='$symbols'}]' >> /dev/null

    # Cleanup
    rm -rf $SCRIPT_DIR/data.sh
    sleep 0.8
}
main;
