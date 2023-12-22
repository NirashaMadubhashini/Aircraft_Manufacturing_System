using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class SizeRepository : Repository<Size>, ISizeRepository
{
    public SizeRepository(DataContext context) : base(context)
    {
    }

    public void Update(Size obj)
    {
        _dbSet.Update(obj);
    }
}