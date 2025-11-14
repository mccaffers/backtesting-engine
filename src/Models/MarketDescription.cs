namespace backtesting_engine_models;

public class MarketDescriptions
{
    public string symbol { get; set; } = "";
    public string IGMarketID { get; set; } = "";
    public string IGMarketIdentifer { get; set; } = "";
    public string currency { get; set; } = "";
    public string IGMarketIdentiferMini { get; set; } = "";
    public string? PolygonIdentifer { get; set; }
    public decimal? PolygonScale { get; set; }
    public MarketType? Type { get; set; }
    public List<StrategyDefinition> strategies { get; set; } = new();
    public decimal? TradeSizeModifier { get; set; } = null;
}

public enum MarketType {
    Forex,
    Indice
}