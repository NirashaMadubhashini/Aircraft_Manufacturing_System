using Aircraft.Models;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface ICartItemRepository : IRepository<CartItem>
{
    void Update(CartItem obj);
}