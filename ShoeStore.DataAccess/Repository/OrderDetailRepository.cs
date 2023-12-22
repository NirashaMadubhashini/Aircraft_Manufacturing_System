using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class OrderDetailRepository : Repository<OrderDetail>, IOrderDetailRepository
{
    public OrderDetailRepository(DataContext context) : base(context)
    {
    }

    public void Update(OrderDetail obj)
    {
        _dbSet.Update(obj);
    }
}