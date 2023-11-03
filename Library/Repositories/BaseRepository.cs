using Library.Context;
using Library.Models;
using Microsoft.EntityFrameworkCore;

namespace Library.Repositories;
public class BaseRepository<TEntity> where TEntity : class, IBaseModel
{
    private readonly DbSet<TEntity> _set;
    private readonly IpContext _dbContext;
    public BaseRepository()
    {
        _dbContext = new IpContext();
        _set = _dbContext.Set<TEntity>();
    }
    public async Task AddAsync(TEntity entity)
    {
        await _set.AddAsync(entity);
        await _dbContext.SaveChangesAsync();
    }
    public TEntity? Get (IQueryable<TEntity> expression)
    {
        return expression.FirstOrDefault();
    }
    public IQueryable<TEntity> CreateQuery()
    {
        return _set.AsNoTracking();
    }
    public void Update(TEntity entity)
    {
        _dbContext.ChangeTracker.Clear();
        _set.Update(entity);
        _dbContext.SaveChanges();
    }
}
