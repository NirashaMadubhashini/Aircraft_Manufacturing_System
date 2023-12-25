using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class ImageRepository : Repository<Image>, IImageRepository
{
    public ImageRepository(DataContext context) : base(context)
    {
    }

    public void Update(Image obj)
    {
        _dbSet.Update(obj);
    }
}