#!/bin/bash
set -ex

export folderPath=/Users/ryan/dev/tickdata/
export symbols=GBPUSD,EURUSD,USDJPY

export GBPUSD_SF=10000
export EURUSD_SF=10000
export USDJPY_SF=100

export accountEquity=500
export maximumDrawndownPercentage=20

export strategy=RandomStrategy

. ./scripts/shared/variables.sh

export runID=$(uuidgen|sed -e 's/-//g')

dotnet build ./src
dotnet test
dotnet run --project ./src