using System.Globalization;
using Utilities;

namespace backtesting_engine_ingest;

public interface IIngest
{
    void EnvironmentSetup();
    Task ReadLines();
}

public class Ingest 
{
    // Simple value and length check on the line
    public static bool ArrayHasRightValues(string[] values)
    {
        return values.Length >= 3 &&
            values.Skip(1).All(x => char.IsDigit(x.FirstOrDefault('a')));
        // just a random default 'a' so that the application doesn't throw on an empty column eg. ,,,,
    }

    // Extract the datetime from the string
    public static (bool parsed, DateTime datetime) extractDt(string dtString)
    {
        DateTime localDt;
        if (dtString.Contains('+'))
        {
            dtString = dtString.Substring(0, dtString.LastIndexOf("+")); // Stripping everything off before the + sign
        }
        var parsedDt = DateTime.TryParseExact(dtString, StringFormats.dtFormat, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out localDt);
        return (parsedDt, localDt);
    }
}
