using System.Diagnostics.CodeAnalysis;

namespace Utilities;

public static class DecimalUtil
{

    private static int[] powof10 = new int[10]
    {
        1,
        10,
        100,
        1000,
        10000,
        100000,
        1000000,
        10000000,
        100000000,
        1000000000
    };

    [SuppressMessage("Sonar Code Smell", "S3776:Cognitive Complexity of methods should not be too high", Justification = "Decimal parser taken from open source for faster parsing")]
    public static decimal ParseDecimal(string input)
    {
        int len = input.Length;
        if (len != 0)
        {
            bool negative = false;
            long n = 0;
            int start = 0;
            if (input[0] == '-')
            {
                negative = true;
                start = 1;
            }
            if (len <= 19)
            {
                int decpos = len;
                for (int k = start; k < len; k++)
                {
                    char c = input[k];
                    if (c == '.')
                    {
                        decpos = k +1;
                    }else{
                        n = (n *10) +(int)(c -'0');
                    }
                }
                return new decimal((int)n, (int)(n >> 32), 0, negative, (byte)(len -decpos));
            }else{
                if (len > 28)
                {
                    len = 28;
                }
                int decpos = len;
                for (int k = start; k < 19; k++)
                {
                    char c = input[k];
                    if (c == '.')
                    {
                        decpos = k +1;
                    }else{
                        n = (n *10) +(int)(c -'0');
                    }
                }
                int n2 = 0;
                bool secondhalfdec = false; 
                for (int k = 19; k < len; k++)
                {
                    char c = input[k];
                    if (c == '.')
                    {
                        decpos = k +1;
                        secondhalfdec = true;
                    }else{
                        n2 = (n2 *10) +(int)(c -'0');
                    }
                }
                byte decimalPosition = (byte)(len -decpos);
                return new decimal((int)n, (int)(n >> 32), 0, negative, decimalPosition) *powof10[len -(!secondhalfdec ? 19 : 20)] +new decimal(n2, 0, 0, negative, decimalPosition);
            }
        }
        return 0;
    }
}