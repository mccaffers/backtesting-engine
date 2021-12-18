#!/bin/bash
set -e

export folderPath=/Users/ryan/dev/tickdata/
export symbols=USDJPY,EURUSD,GBPUSD

dotnet build ./src
dotnet test
dotnet run --project ./src