## C# Backtesting Engine

This is a **hobby project** to experiment with backtesting and algorithm trading. 

### Features
* This engine can ingest multiple symbols in parallel. It uses BufferBlocks with async methods to synchronise them.

### Requirements

- dotnet v6
- Tick data in CSV format, specifically:
    ```bash
    # ./tickdata/{symbol}/2020.csv:
    UTC,AskPrice,BidPrice,AskVolume,BidVolume
    2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75
    ```

### Setup
1. Update ```path``` in ```deploy.sh``` to reference your tick folders
    * Your csv files should be contained within a symbol folder eg. ```./tickdata/{symbol}/2020.csv```
2. Update ```symbols``` in ```deploy.sh``` to reference all the symbols you wish to backtest in parallel
3. Run ```deploy.sh```

### License
[MIT](https://choosealicense.com/licenses/mit/)