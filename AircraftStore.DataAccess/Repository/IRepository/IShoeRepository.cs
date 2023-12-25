using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IShoeRepository : IRepository<Shoe>
{
    void Update(Shoe obj);
}