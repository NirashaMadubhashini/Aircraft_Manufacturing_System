using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class ShoeRepository : Repository<Shoe>, IShoeRepository
{
    public ShoeRepository(DataContext context) : base(context)
    {
    }

    public void Update(Shoe obj)
    {
        _dbSet.Update(obj);
    }
}