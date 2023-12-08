<div align="center">
<h3 align="center">C# Backtesting Engine</h3>
  <p align="center">
    By McCaffers
  </p>
</div>

<!-- ABOUT THE PROJECT -->

## About The Project

This backtesting engine is a personal project of mine, built in C# to explore and experiment with various trading strategies at scale.

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mccaffers_backtesting-engine&metric=alert_status)](https://sonarcloud.io/summary/overall?id=mccaffers_backtesting-engine)

### Features
* Multiple symbol ingest with time synchronisation
* xUnit testing 
* Trade Environment
    * Trade Excution
    * Equity Monitoring
* Reporting (ElasticSearch)

### Built With

* [dotnet](https://dotnet.com)

<!-- GETTING STARTED -->
### Getting Started

#### Testing
`dotnet test`

#### Demo Run (Working)
{placeholder}

#### Local Terminal
sh ./scripts/backtesting/run.sh

#### Local Web & Terminal
sh ./script/backtesting/web.sh

### Dependencies

* dotnet v6
  * Charting & Web use, need to accept dotnet's certificates `dotnet dev-certs https --trust`
* ElasticSearch for reporting
* CanvasJS used for charting
  * JS libaries `canvasjs.min.js` to be placed within ./src/ui/src/libs/ folder
* Some financial tick data in CSV format, in the /src/tickdata folder (example provided)

    ```bash
    # ./tickdata/{symbol}/2020.csv:
    UTC,AskPrice,BidPrice,AskVolume,BidVolume
    2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75
    ```

### License
[MIT](https://choosealicense.com/licenses/mit/)

<p align="right">(<a href="#top">back to top</a>)</p>
