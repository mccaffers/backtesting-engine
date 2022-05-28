namespace backtesting_engine;

class InitialReport {
    public string hostname {get;set;} = string.Empty;
    public DateTime date {get;set;}
    public string[] symbols {get;set;} = new string[]{};
    public string runID {get;set;} = string.Empty;
    public int runIteration {get;set;}
    public int instanceCount {get;set;} = 0;
    public string strategy {get;set;} = string.Empty;
}

class YearUpdate : InitialReport  {
    public int year {get;set;} = 0;
}