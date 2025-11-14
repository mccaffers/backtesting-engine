using MathNet.Numerics.Distributions;

namespace Utilities;

public class ConfidenceObj
{
    public double lowerLimit { get; set; }
    public double upperLimit { get; set; }
    public double mean { get; set; }
    public double stdDev { get; set; }
    public double normalDistributionMean { get; set; }
    public double criticalValue { get; set; }
}

public class StandardDeviationConfidenceIntervalCalculator
{
    // This method calculates the confidence interval for a given set of financial data
    public static ConfidenceObj Calculate(double[] data, double confidenceLevel)
    {
        // Calculate the mean (average) of the data
        // This represents the central tendency of the financial data
        double mean = data.Average();

        // Calculate the standard deviation of the data
        // This measures the amount of variation or dispersion of the data points
        double stdDev = data.StandardDeviation();

        // Create a normal distribution object using the calculated mean and standard deviation
        // This assumes that the financial data follows a normal distribution
        Normal normalDistribution = Normal.WithMeanStdDev(mean, stdDev);

        // Calculate the critical value based on the confidence level
        // This value is used to determine the width of the confidence interval
        // (1 + confidenceLevel) / 2 is used to get the upper tail probability
        // For example, for a 95% confidence level, we use 0.975 (two-tailed test)
        double criticalValue = normalDistribution.InverseCumulativeDistribution((1 + confidenceLevel) / 2);

        // Calculate the lower limit of the confidence interval
        // This formula is derived from the Central Limit Theorem
        double lowerLimit = mean - (criticalValue * (stdDev / Math.Sqrt(data.Length)));

        // Calculate the upper limit of the confidence interval
        double upperLimit = mean + (criticalValue * (stdDev / Math.Sqrt(data.Length)));

        // Return the confidence interval as a tuple
        return new ConfidenceObj() {
            lowerLimit=lowerLimit,
            upperLimit=upperLimit,
            criticalValue=criticalValue,
            mean=mean,
            stdDev=stdDev,
            normalDistributionMean = normalDistribution.Mean

        };
    }
}

public static class Extend
{
    public static double StandardDeviation(this IEnumerable<double> values)
    {
        double avg = values.Average();
        return Math.Sqrt(values.Average(v => Math.Pow(v - avg, 2)));
    }
}
