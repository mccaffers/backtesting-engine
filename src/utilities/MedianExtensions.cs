public static class MedianExtensions
{
#pragma warning disable CS8629 // Nullable value type may be null.
    public static double Median(this IEnumerable<int> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<int?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double Median(this IEnumerable<long> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<long?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static float Median(this IEnumerable<float> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
        return data[data.Length / 2];
    }
    
    public static float? Median(this IEnumerable<float?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0f;
        return data[data.Length / 2];
    }
    
    public static double Median(this IEnumerable<double> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static double? Median(this IEnumerable<double?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0;
        return data[data.Length / 2];
    }
    
    public static decimal Median(this IEnumerable<decimal> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");
        var data = source.OrderBy(n => n).ToArray();
        if (data.Length == 0)
            throw new InvalidOperationException();
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
        return data[data.Length / 2];
    }
    
    public static decimal? Median(this IEnumerable<decimal?> source)
    {
        if (source == null)
            throw new ArgumentNullException("source");

        var data = source.Where(n => n.HasValue).Select(n => n.Value).OrderBy(n => n).ToArray();
        if (data.Length == 0)
            return null;
        if (data.Length % 2 == 0)
            return (data[data.Length / 2 - 1] + data[data.Length / 2]) / 2.0m;
        return data[data.Length / 2];
    }

    #pragma warning restore CS8629 // Nullable value type may be null.
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, int> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, int?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, long> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, long?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static float Median<TSource>(this IEnumerable<TSource> source, Func<TSource, float> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static float? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, float?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double Median<TSource>(this IEnumerable<TSource> source, Func<TSource, double> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static double? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, double?> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static decimal Median<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal> selector)
    {
        return source.Select(selector).Median();
    }
    
    public static decimal? Median<TSource>(this IEnumerable<TSource> source, Func<TSource, decimal?> selector)
    {
        return source.Select(selector).Median();
    }
}