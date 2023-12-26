using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IAirplaneRepository : IRepository<Airplane>
{
    void Update(Airplane obj);
}