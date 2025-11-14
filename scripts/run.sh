#!/bin/bash

dotnet publish ./src/backtesting

json='{
  "RUN_ID": "LOCAL#RANDOM_ID#",
  "SYMBOLS": "EURUSD",
  "LAST_MONTHS": 2,
  "REPORT_INDIVIDUAL_TRADES": false,
  "REPORT_TO_ELASTICSEARCH": true,
  "REPORT_LOSSES": true,
  "CONSOLE_LOG": true,
  "SYSTEM_LOG": true,
  "STRATEGY": {
     "UUID": "EURUSD#STRATEGY#RANDOM_ID#",
      "TRADING_VARIABLES": {
        "STRATEGY": "RandomStrategy",
        "STOP_DISTANCE_IN_PIPS": "100",
        "LIMIT_DISTANCE_IN_PIPS": "9",
        "TRAILING_STOP_LOSS_ACTIVE": "0",
        "TRAILING_STOP_LOSS_VALUE": "0",
        "MOVING_LIMIT_VALUE": "0",
        "TRADING_SIZE": "1"
      },
      "OHLC_GROUP_DISTANCE_VARIABLES": {
        "DISTANCE_FROM_GROUP": "44",
        "GROUPS_MUST_BE_WITHIN_PIPS": "32",
        "THRESHOLD_GROUP_MINIMUM": "17",
        "GROUP_MINUTES": "100"
      }
  }
}'

# Base64 encode the JSON and store in payload
export payload=$(echo "$json" | base64)

export ELASTICSEARCH_URI="http://localhost:9200"
export ELASTICSEARCH_USERNAME="TEST"
export ELASTICSEARCH_PASSWORD="TEST"

dotnet run --project ./src/backtesting