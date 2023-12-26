using Microsoft.EntityFrameworkCore.Storage;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;

namespace Aircraft.DataAccess.Repository;

public class UnitOfWork : IUnitOfWork
{
    private readonly DataContext _context;

    public UnitOfWork(DataContext context)
    {
        _context = context;
        Brands = new BrandRepository(_context);
        Colors = new ColorRepository(_context);
        Sizes = new SizeRepository(_context);
        Categories = new CategoryRepository(_context);
        Airplanes = new AirplaneRepository(_context);
        AirplaneColors = new AirplaneColorRepository(_context);
        Images = new ImageRepository(_context);
        AirplaneSizes = new AirplaneSizeRepository(_context);
        CartItems = new CartItemRepository(_context);
        ApplicationUsers = new ApplicationUserRepository(_context);
        Orders = new OrderRepository(_context);
        OrderDetails = new OrderDetailRepository(_context);
    }

    public IBrandRepository Brands { get; }
    public IColorRepository Colors { get; }
    public ISizeRepository Sizes { get; }
    public ICategoryRepository Categories { get; }
    public IAirplaneRepository Airplanes { get; }
    public IAirplaneColorRepository AirplaneColors { get; }
    public IImageRepository Images { get; }
    public IAirplaneSizeRepository AirplaneSizes { get; }
    public ICartItemRepository CartItems { get; }
    public IApplicationUserRepository ApplicationUsers { get; }
    public IOrderRepository Orders { get; }
    public IOrderDetailRepository OrderDetails { get; }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
    
    public async Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return await _context.Database.BeginTransactionAsync();
    }
    // public async Task CommitTransactionAsync()
    // {
    //     await _context.Database.CommitTransactionAsync();
    // }
}