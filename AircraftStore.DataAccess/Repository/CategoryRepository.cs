using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class CategoryRepository : Repository<Category>, ICategoryRepository
{
    public CategoryRepository(DataContext context) : base(context)
    {
    }

    public void Update(Category obj)
    {
        _dbSet.Update(obj);
    }
}