#!/bin/bash
set -ex

set -o allexport
source ./.env/local.env
set +o allexport

runID=$(uuidgen|sed -e 's/-//g')

dotnet build ./src
dotnet test
dotnet run --project ./src