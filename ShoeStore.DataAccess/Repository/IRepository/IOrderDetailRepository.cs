using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IOrderDetailRepository : IRepository<OrderDetail>
{
    void Update(OrderDetail obj);
}