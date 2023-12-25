using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IShoeSizeRepository : IRepository<ShoeSize>
{
    void Update(ShoeSize obj);
}