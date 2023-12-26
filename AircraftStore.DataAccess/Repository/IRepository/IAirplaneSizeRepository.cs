using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IAirplaneSizeRepository : IRepository<AirplaneSize>
{
    void Update(AirplaneSize obj);
}