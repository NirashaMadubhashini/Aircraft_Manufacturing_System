using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class ShoeSizeRepository : Repository<ShoeSize>, IShoeSizeRepository
{
    public ShoeSizeRepository(DataContext context) : base(context)
    {
    }

    public void Update(ShoeSize obj)
    {
        _dbSet.Update(obj);
    }
}