using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IColorRepository : IRepository<Color>
{
    void Update(Color obj);
}