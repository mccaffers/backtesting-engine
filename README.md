<div align="center">

<h3 align="center">C# Backtesting Engine</h3>

  <p align="center">
    By McCaffers

  </p>
</div>

<!-- ABOUT THE PROJECT -->
## About The Project

This backtesting engine is a personal project of mine, built in C# to explore and experiment with various trading strategies at scale.

### Features
* Multiple symbol ingest and time synchronisation
* Sonar analysis -> [link](https://sonarcloud.io/project/overview?id=mccaffers_backtesting-engine)

### Roadmap
* xUnit testing
* Trade Environment
    * Trade Excution
    * Equity Monitoring
* Reporting

### Built With

* [dotnet](https://nextjs.org/)

<!-- GETTING STARTED -->
## Getting Started

### Prerequisites

* You need dotnet v6
* You need tick data in CSV format, specifically:
    ```bash
    # ./tickdata/{symbol}/2020.csv:
    UTC,AskPrice,BidPrice,AskVolume,BidVolume
    2018-01-01T01:00:00.594+00:00,1.35104,1.35065,1.5,0.75
    ```

### Deploy

1. Review variables in ```./scripts/config```, updating the path and symbols you wish to backtest
2. Run ```deploy.sh``` which builds and runs the dotnet application locally

### License
[MIT](https://choosealicense.com/licenses/mit/)

<p align="right">(<a href="#top">back to top</a>)</p>
