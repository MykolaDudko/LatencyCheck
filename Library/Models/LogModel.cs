using System.Net;

namespace Library.Models;
public class LogModel : BaseModel
{
    public IPAddress Ip { get; set; }
    public double AverageResponseTime { get; set; }
    public Guid Key { get; set; }
}
