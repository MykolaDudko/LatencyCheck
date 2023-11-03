using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Context;
public class IpContext : DbContext
{
    public DbSet<LogModel> Logs { get; set; }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=mydatabase.db");
    }
}
