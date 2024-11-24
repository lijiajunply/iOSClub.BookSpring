namespace TencentCos.Lib.Common;

public static class Extension
{
    public static long ToUnixTime(this DateTime nowTime)
    {
        var startTime = new DateTime(1970, 1, 1, 0, 0, 0, 0);
        return (long)Math.Round((nowTime - startTime).TotalMilliseconds, MidpointRounding.AwayFromZero);
    }
}