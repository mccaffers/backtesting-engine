#!/bin/bash
set -e

set -o allexport
source ./.env/local.env
set +o allexport

runID=$(uuidgen|sed -e 's/-//g')
# echo $runID

dotnet build ./src/backtesting

## Function to kill any running dotnet processes
## when the script closes
_term() { 
  # Ensure dotnet exits
    echo "pkill -f './src/backtesting'"
    pkill -f "./src/backtesting"; echo $?
    echo """EXIT STATUS - (pkill dotnet)
0      One or more processes matched the criteria.
1      No processes matched"""
}

## Ensure all previous dotnet processes
## have closed before starting a new one
_term || true

trap _term SIGINT SIGTERM

## Ensure all of the dependencies in the UI folder 
## have been installed
npm install --prefix ./src/ui

## Use gnu parallel to launch both processes together
## and half if either one `fail=1` was to close
parallel --halt now,fail=1 --line-buffer --tty --jobs 2 --verbose ::: \
"dotnet run --project ./src/backtesting -- web" \
"npm start --prefix ./src/ui/ 1> /dev/null 2> /dev/null" 

