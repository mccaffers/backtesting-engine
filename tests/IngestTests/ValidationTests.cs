using backtesting_engine_ingest;
using Xunit;

namespace Tests;
public class IngestValidationTests
{

    
    [Theory]
    [InlineData(true, new string[] { "2018-01-01T22:52:26.862+00:00", "1.20146", "1.20138","1.2","1.3"})]
    [InlineData(false, new string[] {})]
    [InlineData(false, new string[] { "" })]
    [InlineData(false, new string[] { "","","","","" })]
    [InlineData(false, new string[] { "a","a","a","a","a" })]
    [InlineData(false, new string[] { "a","1.2","a","a","a" })]
    public void CheckValidation(bool expectedResult, string[] strings)
    {
        // Act
        var outputResult = Ingest.ArrayHasRightValues(strings);
        Assert.Equal(expectedResult, outputResult);
    }

    [Theory]
    [InlineData(true, "2018-01-01T22:52:26.862+00:00")]
    [InlineData(true, "2018-01-01T22:52:26.862")]
    [InlineData(false, "2018-01-01 22:52:26.862")]
    [InlineData(false, "2018-01-01")]
    [InlineData(false, "a")]
    public void CheckDateExtract(bool expectedResult, string dateTimeString)
    {
        // Act
        var outputResult = Ingest.extractDt(dateTimeString).parsed;
        Assert.Equal(expectedResult, outputResult);
    }

}