using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IBrandRepository : IRepository<Brand>
{
    void Update(Brand obj);
}