using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface ISizeRepository : IRepository<Size>
{
    void Update(Size obj);
}