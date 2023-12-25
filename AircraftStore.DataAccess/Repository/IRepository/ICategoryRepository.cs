using System.Linq.Expressions;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface ICategoryRepository : IRepository<Category>
{
    void Update(Category obj);

}