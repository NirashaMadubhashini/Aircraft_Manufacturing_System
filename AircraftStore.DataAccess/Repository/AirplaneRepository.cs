using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class AirplaneRepository : Repository<Airplane>, IAirplaneRepository
{
    public AirplaneRepository(DataContext context) : base(context)
    {
    }

    public void Update(Airplane obj)
    {
        _dbSet.Update(obj);
    }
}