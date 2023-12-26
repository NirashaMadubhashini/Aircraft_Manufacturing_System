using Aircraft.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Aircraft.DataAccess.Data;

public class DataContext : IdentityDbContext
{
    public DataContext(DbContextOptions<DataContext> options) : base(options)
    {
    }

    public DbSet<Brand> Brands => Set<Brand>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Color> Colors => Set<Color>();
    public DbSet<Size> Sizes => Set<Size>();
    public DbSet<Airplane> Airplanes => Set<Airplane>();
    public DbSet<Image> Images => Set<Image>();
    public DbSet<AirplaneColor> AirplaneColor => Set<AirplaneColor>();
    public DbSet<AirplaneSize> AirplaneSize => Set<AirplaneSize>();
    public DbSet<ApplicationUser> ApplicationUsers => Set<ApplicationUser>();
    public DbSet<CartItem> CartItems => Set<CartItem>();
    public DbSet<ShopOrder> Orders => Set<ShopOrder>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(x => x.Id);
            // entity.Property(x => x.Unit);
            entity.HasOne(x => x.Parent)
                .WithMany(x => x.Children)
                .HasForeignKey(x => x.ParentId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);
        });
    }
}