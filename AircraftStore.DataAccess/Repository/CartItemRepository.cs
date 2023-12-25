using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class CartItemRepository : Repository<CartItem>, ICartItemRepository
{
    public CartItemRepository(DataContext context) : base(context)
    {
    }

    public void Update(CartItem obj)
    {
        _dbSet.Update(obj);
    }
}