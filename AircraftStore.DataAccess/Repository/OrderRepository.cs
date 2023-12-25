using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;

namespace Aircraft.DataAccess.Repository;

public class OrderRepository : Repository<ShopOrder>, IOrderRepository
{
    public OrderRepository(DataContext context) : base(context)
    {
    }

    public void Update(ShopOrder obj)
    {
        _dbSet.Update(obj);
    }

    public async Task UpdateStatusAsync(int id, string orderStatus, string? paymentStatus = null)
    {
        ShopOrder? orderFromDb = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);

        if (orderFromDb != null)
        {
            orderFromDb.OrderStatus = orderStatus;
            if (paymentStatus != null)
            {
                orderFromDb.PaymentStatus = paymentStatus;
            }
        }
    }

    public async Task UpdateStripePaymentId(int id, string sessionId, string? paymentIntentId)
    {
        var orderFromDb = await _dbSet.FirstOrDefaultAsync(e => e.Id == id);

        if (orderFromDb == null)
        {
            return;
        }

        orderFromDb.PaymentDate = DateTime.Now;
        orderFromDb.SessionId = sessionId;
        orderFromDb.PaymentIntentId = paymentIntentId;

        await _context.SaveChangesAsync();
    }
}