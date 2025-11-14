

using backtesting_engine;
using backtesting_engine_models;

namespace backtesting_engine_ingest;

public class BuildStrategy
{

    public static StrategyDefinition Generate() {
        return Program.BACKTESTING_STRATEGY_DEFINITION!;
    }

}