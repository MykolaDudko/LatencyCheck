namespace Library.Models;

public class RateLimit
{
    public string rateLimitType { get; set; }
    public string interval { get; set; }
    public int intervalNum { get; set; }
    public int limit { get; set; }
    public int count { get; set; }
}

public class Result
{
    public long serverTime { get; set; }
}

public class CheckServerTimeModel
{
    public string id { get; set; }
    public int status { get; set; }
    public Result result { get; set; }
    public List<RateLimit> rateLimits { get; set; }
}
