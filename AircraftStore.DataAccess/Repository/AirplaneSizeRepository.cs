using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class AirplaneSizeRepository : Repository<AirplaneSize>, IAirplaneSizeRepository
{
    public AirplaneSizeRepository(DataContext context) : base(context)
    {
    }

    public void Update(AirplaneSize obj)
    {
        _dbSet.Update(obj);
    }
}