#!/bin/bash
set -ex

set -o allexport
source ./.env/local.env
set +o allexport

runID=$(uuidgen|sed -e 's/-//g')

dotnet build ./src/backtesting
# dotnet test

echo $runID

parallel --halt now,fail=1 --line-buffer --tty --jobs 2 --verbose ::: \
"dotnet run --project ./src/backtesting -- web" \
"npm start --prefix ./src/ui/ 1> /dev/null 2> /dev/null" 




