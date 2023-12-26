using Microsoft.EntityFrameworkCore.Storage;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IUnitOfWork
{
    IBrandRepository Brands { get; }
    IColorRepository Colors { get; }
    ISizeRepository Sizes { get; }
    ICategoryRepository Categories { get; }
    IAirplaneRepository Airplanes { get; }
    IAirplaneColorRepository AirplaneColors { get; }
    IImageRepository Images { get; }
    IAirplaneSizeRepository AirplaneSizes { get; }
    ICartItemRepository CartItems { get; }
    IApplicationUserRepository ApplicationUsers { get; }
    IOrderRepository Orders { get; }
    IOrderDetailRepository OrderDetails { get; }

    Task SaveChangesAsync();
    
    public Task<IDbContextTransaction> BeginTransactionAsync();
    // public Task CommitTransactionAsync();
}