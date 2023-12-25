using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class BrandRepository : Repository<Brand>, IBrandRepository
{
    public BrandRepository(DataContext context) : base(context)
    {
    }

    public void Update(Brand obj)
    {
        _dbSet.Update(obj);
    }

}