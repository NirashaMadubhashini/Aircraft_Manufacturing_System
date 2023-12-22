using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IImageRepository : IRepository<Image>
{
    void Update(Image obj);
}