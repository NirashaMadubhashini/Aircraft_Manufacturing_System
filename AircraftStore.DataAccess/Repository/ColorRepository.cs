using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class ColorRepository : Repository<Color>, IColorRepository
{
    public ColorRepository(DataContext context) : base(context)
    {
    }

    public void Update(Color obj)
    {
        _dbSet.Update(obj);
    }
}