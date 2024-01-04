#!/bin/bash
set -e

set -o allexport
source ./.env/local.env
set +o allexport

runID=$(uuidgen|sed -e 's/-//g')

dotnet build ./src/backtesting
# dotnet test

echo $runID

_term() { 
  # Ensure dotnet exits
    echo "pkill dotnet"
    pkill dotnet ; echo $?
    echo """EXIT STATUS - (pkill dotnet)
0      One or more processes matched the criteria.
1      No processes matched"""
}

trap _term SIGINT SIGTERM

parallel --halt now,fail=1 --line-buffer --tty --jobs 2 --verbose ::: \
"dotnet run --project ./src/backtesting -- web" \
"npm start --prefix ./src/ui/ 1> /dev/null 2> /dev/null" 

