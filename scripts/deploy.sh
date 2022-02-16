#!/bin/bash
set -e

export folderPath=/Users/ryan/dev/tickdata/
export symbols=GBPUSD #,EURUSD,USDJPY

export GBPUSD_SF=10000
export EURUSD_SF=10000
export USDJPY_SF=100

dotnet build ./src
dotnet test
dotnet run --project ./src