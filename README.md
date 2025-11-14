## C# Backtesting Engine

> [!NOTE]
> This project is maintained but no longer under active development. It receives periodic updates and improvements, but my primary focus has shifted to a **C++ implementation** for further experimentation. Check out the progress on the high-performance C++ version here: **[backtesting-engine-cpp](https://github.com/mccaffers/backtesting-engine-cpp)**

### Background

This is a high-performance C# backtesting engine designed to analyze financial data and evaluate multiple trading strategies at scale. The engine leverages **QuestDB** for efficient time-series data storage and retrieval, providing significantly faster query performance and more robust handling of tick-level financial data compared to traditional file-based approaches. This project serves as a practical demonstration of quantitative engineering capabilities, combining financial domain knowledge with modern software architecture and cloud infrastructure.

> I initiated this project to deepen my understanding of financial markets while showcasing technical expertise through detailed documentation and transparent decision-making. By building a production-grade backtesting system from the ground up I've been able to explore real-world challenges in quantitative finance—from data pipeline optimisation to strategy evaluation frameworks. The project also initially investigates the practical benefits of cloud services, specifically AWS, for horizontally scaling strategy permutations and experiments. <br/><br/> This cloud-native approach dramatically reduces the time required to analyze results and generate actionable insights, enabling rapid iteration on trading hypotheses and more comprehensive strategy exploration than would be feasible on local infrastructure for better cost and performance. <br/></br>Do read more of my blog detailing the development process: https://mccaffers.com/quantitative_engineering/building_a_backtesting_system/

![alt](images/development_active.svg) [![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=mccaffers_backtesting-engine&metric=alert_status)](https://sonarcloud.io/summary/overall?id=mccaffers_backtesting-engine) [![Build](https://github.com/mccaffers/backtesting-engine/actions/workflows/build.yml/badge.svg)](https://github.com/mccaffers/backtesting-engine/actions/workflows/build.yml) [![Bugs](https://sonarcloud.io/api/project_badges/measure?project=mccaffers_backtesting-engine&metric=bugs)](https://sonarcloud.io/summary/new_code?id=mccaffers_backtesting-engine) [![Coverage](https://sonarcloud.io/api/project_badges/measure?project=mccaffers_backtesting-engine&metric=coverage)](https://sonarcloud.io/summary/new_code?id=mccaffers_backtesting-engine)

### Results
Results are extracted and analyzed using various visualization tools for trend analysis.

![alt text](images/random-indices-sp500-variable.svg)

*Read more results on https://mccaffers.com/randomly_trading/*

### Features

- [x] **QuestDB Integration** - High-performance time-series database for tick data
- [x] 24 xUnit tests covering Trade Management, Ingest, Reporting & Utilities
- [x] Trade Environment
    * Trade Execution
    * Equity Monitoring
    * Position Management
- [x] Reporting (ElasticSearch)
- [x] Environment-based configuration

## Getting Started

### Prerequisites

* .NET 9
* QuestDB (localhost:8812 for jdbc client)
* ElasticSearch for reporting (optional, will report to terminal if not present)

### Running the Engine

1. **Start QuestDB** locally (follow [QuestDB installation guide](https://questdb.io/docs/get-started/docker/))

2. **Configure your strategy** - Edit the JSON payload in `./scripts/run.sh` to set:
   - Trading symbols (e.g., EURUSD)
   - Strategy parameters (stop distance, limit distance, etc.)
   - Backtesting period (LAST_MONTHS)
   - Environment variables (Elasticsearch URI, username, password)

3. **Run the engine**:
   ```bash
   ./scripts/run.sh
   ```

4. **Run tests**:
   ```bash
   dotnet test
   ```
   Check, 24 tests should pass successfully.

## Why QuestDB?

QuestDB offers several advantages for financial backtesting:
- **Fast ingestion** - Optimized for high-frequency time-series data
- **SQL interface** - Familiar query language with time-series extensions
- **Low latency** - Microsecond-level query performance
- **Efficient storage** - Column-oriented storage reduces disk footprint

## Configuration

Environment variables are managed through the JSON payload in `./scripts/run.sh`:

```json
{
  "RUN_ID": "LOCAL#RANDOM_ID#",
  "SYMBOLS": "EURUSD",
  "LAST_MONTHS": 2,
  "REPORT_TO_ELASTICSEARCH": true,
  "STRATEGY": {
    "TRADING_VARIABLES": {
      "STRATEGY": "RandomStrategy",
      "STOP_DISTANCE_IN_PIPS": "100",
      "LIMIT_DISTANCE_IN_PIPS": "9"
    }
  }
}
```

Set your Elasticsearch credentials:
```bash
export ELASTICSEARCH_URI="http://localhost:9200"
export ELASTICSEARCH_USERNAME="your_username"
export ELASTICSEARCH_PASSWORD="your_password"
```

## Contributing

Issues and pull requests are welcome! Please raise any bugs or feature requests on the [GitHub repository](https://github.com/mccaffers/backtesting-engine/issues).

## License

[MIT](https://choosealicense.com/licenses/mit/)
