#!/bin/bash
set -ex

set -o allexport
source ./.env/local.env
set +o allexport

runID=$(uuidgen|sed -e 's/-//g')

dotnet build ./src/backtesting
# dotnet test

echo $runID
dotnet run --project ./src/backtesting -- web