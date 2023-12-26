using System.Linq.Expressions;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Models.ViewModels;

namespace Aircraft.DataAccess.Repository;

public class AirplaneColorRepository : Repository<AirplaneColor>, IAirplaneColorRepository
{
    public AirplaneColorRepository(DataContext context) : base(context)
    {
    }

    public void Update(AirplaneColor obj)
    {
        _dbSet.Update(obj);
    }

    public async Task<ProductListViewModel> FilterProductAsync(int pageSize,
        string? productName = null, int page = 1, string? brand = null,
        int? size = null,
        decimal? minPrice = null, decimal? maxPrice = null, string? sort = "latest")
    {
        var airplaneQuery = _context.Airplanes
            .Include(e => e.Brand)
            .Select(e => new
            {
                AirplaneId = e.Id,
                Name = e.Name,
                BrandName = e.Brand!.Name
            });
        if (brand != null)
        {
            airplaneQuery = airplaneQuery.Where(e => e.BrandName == brand);
        }

        if (productName != null)
        {
            airplaneQuery = airplaneQuery.Where(e => EF.Functions.Like(e.Name, $"%{productName.Trim()}%"));
        }

        var airplaneColorsQuery = _context.AirplaneColor
            .Where(e => e.Active == true)
            .Include(e => e.Images)
            .Include(e => e.AirplaneSizes)
            .Select(e => new
            {
                AirplaneId = e.AirplaneId,
                AirplaneSizes = e.AirplaneSizes,
                Price = e.SalePrice,
                ImagePath = e.Images.First().Path,
                Url = e.Url,
                Created = e.Created
            });

        if (size != null)
        {
            airplaneColorsQuery = airplaneColorsQuery.Where(e => e.AirplaneSizes.Any(e => e.Size.Value == size));
        }

        ;
        if (maxPrice != null)
        {
            airplaneColorsQuery = airplaneColorsQuery.Where(e => e.Price <= maxPrice);
        }

        IQueryable<ProductCardViewModel> productCardViewModels = from airplaneColor in airplaneColorsQuery
                                                                 join airplane in airplaneQuery
                on airplaneColor.AirplaneId equals airplane.AirplaneId
                                                                 select new ProductCardViewModel()
            {
                Name = airplane.Name, Price = airplaneColor.Price, Url = airplaneColor.Url, BrandName = airplane.BrandName,
                ImagePath = airplaneColor.ImagePath, Created = airplaneColor.Created
            };

        switch (sort)
        {
            case "lowest":
                productCardViewModels = productCardViewModels.OrderBy(e => e.Price);
                break;
            case "highest":
                productCardViewModels = productCardViewModels.OrderBy(e => e.Price).Reverse();
                break;
            default:
                sort = "latest";
                productCardViewModels = productCardViewModels.OrderBy(e => e.Created);
                break;
        }

        List<ProductCardViewModel> productCards = await productCardViewModels.ToListAsync();

        var brands = await _context.Brands.AsNoTracking().OrderBy(e => e.Name).ToListAsync();
        var sizes = await _context.Sizes.AsNoTracking().OrderBy(e => e.Value).ToListAsync();

        ProductListViewModel productListViewModel = new ProductListViewModel()
        {
            Brands = brands,
            Sizes = sizes,
            // AirplaneColors = airplaneColors,
            ProductCards = productCards
                .Skip(pageSize * (page - 1))
                .Take(pageSize)
                .ToList(),
            SelectedBrandId = (brands.FirstOrDefault(e => e.Name == brand))?.Id,
            SelectedSizeId = (sizes.FirstOrDefault(e => e.Value.ToString() == size.ToString()))?.Id,
            PagingInfo = new PagingInfo()
            {
                CurrentPage = page,
                ItemsPerPage = pageSize,
                TotalItems = productCards.Count
            },
            CurrentBrand = brand
        };

        if (maxPrice != null)
        {
            productListViewModel.maxPrice = maxPrice;
        }

        productListViewModel.SearchedBrand = brand;

        List<SelectListItem> selectListItems = new List<SelectListItem>()
        {
            new SelectListItem("Latest", "latest"),
            new SelectListItem("Price: Low to high", "lowest"),
            new SelectListItem("Price: High to low", "highest"),
        };
        for (int i = 0; i < selectListItems.Count; i++)
        {
            SelectListItem opt = selectListItems[i];
            if (opt.Value == sort)
            {
                opt.Selected = true;
                break;
            }
        }

        productListViewModel.SelectListItems = selectListItems;


        return productListViewModel;
    }

    // public async Task<ProductListViewModel> FilterProductAsync(int pageSize,
    //     string? productName = null, int page = 1, string? brand = null,
    //     int? size = null,
    //     decimal? minPrice = null, decimal? maxPrice = null, string? sort = "latest")
    // {
    //     var airplaneQuery = _context.Airplanes
    //         .Include(e => e.Brand)
    //         .Select(e => new
    //         {
    //             AirplaneId = e.Id,
    //             Name = e.Name,
    //             BrandName = e.Brand!.Name
    //         });
    //     if (brand != null)
    //     {
    //         airplaneQuery = airplaneQuery.Where(e => e.BrandName == brand);
    //     }
    //
    //     if (productName != null)
    //     {
    //         airplaneQuery = airplaneQuery.Where(e => EF.Functions.Like(e.Name, $"%{productName.Trim()}%"));
    //     }
    //
    //     var airplaneColorsQuery = _context.AirplaneColor
    //         .Include(e => e.Images)
    //         .Include(e => e.AirplaneSizes)
    //         .Select(e => new
    //         {
    //             AirplaneId = e.AirplaneId,
    //             AirplaneSizes = e.AirplaneSizes,
    //             Price = e.SalePrice,
    //             ImagePath = e.Images.First().Path,
    //             Url = e.Url,
    //             Created = e.Created
    //         });
    //
    //     if (size != null)
    //     {
    //         airplaneColorsQuery = airplaneColorsQuery.Where(e => e.AirplaneSizes.Any(e => e.Size.Value == size));
    //     }
    //
    //     ;
    //     if (maxPrice != null)
    //     {
    //         airplaneColorsQuery = airplaneColorsQuery.Where(e => e.Price <= maxPrice);
    //     }
    //
    //     IQueryable<ProductCardViewModel> productCardViewModels = from airplaneColor in airplaneColorsQuery
    //         join airplane in airplaneQuery
    //             on airplaneColor.AirplaneId equals airplane.AirplaneId
    //         select new ProductCardViewModel()
    //         {
    //             Name = airplane.Name, Price = airplaneColor.Price, Url = airplaneColor.Url, BrandName = airplane.BrandName,
    //             ImagePath = airplaneColor.ImagePath, Created = airplaneColor.Created
    //         };
    //
    //     switch (sort)
    //     {
    //         case "lowest":
    //             productCardViewModels = productCardViewModels.OrderBy(e => e.Price);
    //             break;
    //         case "highest":
    //             productCardViewModels = productCardViewModels.OrderBy(e => e.Price).Reverse();
    //             break;
    //         default:
    //             sort = "latest";
    //             productCardViewModels = productCardViewModels.OrderBy(e => e.Created);
    //             break;
    //     }
    //
    //     List<ProductCardViewModel> productCards = await productCardViewModels.ToListAsync();
    //
    //     var brands = await _context.Brands.AsNoTracking().OrderBy(e => e.Name).ToListAsync();
    //     var sizes = await _context.Sizes.AsNoTracking().OrderBy(e => e.Value).ToListAsync();
    //
    //     ProductListViewModel productListViewModel = new ProductListViewModel()
    //     {
    //         Brands = brands,
    //         Sizes = sizes,
    //         // AirplaneColors = airplaneColors,
    //         ProductCards = productCards
    //             .Skip(pageSize * (page - 1))
    //             .Take(pageSize)
    //             .ToList(),
    //         SelectedBrandId = (brands.FirstOrDefault(e => e.Name == brand))?.Id,
    //         SelectedSizeId = (sizes.FirstOrDefault(e => e.Value.ToString() == size.ToString()))?.Id,
    //         PagingInfo = new PagingInfo()
    //         {
    //             CurrentPage = page,
    //             ItemsPerPage = pageSize,
    //             TotalItems = productCards.Count
    //         },
    //         CurrentBrand = brand
    //     };
    //
    //     if (maxPrice != null)
    //     {
    //         productListViewModel.maxPrice = maxPrice;
    //     }
    //
    //     productListViewModel.SearchedBrand = brand;
    //
    //     List<SelectListItem> selectListItems = new List<SelectListItem>()
    //     {
    //         new SelectListItem("Latest", "latest"),
    //         new SelectListItem("Price: Low to high", "lowest"),
    //         new SelectListItem("Price: High to low", "highest"),
    //     };
    //     for (int i = 0; i < selectListItems.Count; i++)
    //     {
    //         SelectListItem opt = selectListItems[i];
    //         if (opt.Value == sort)
    //         {
    //             opt.Selected = true;
    //             break;
    //         }
    //     }
    //
    //     productListViewModel.SelectListItems = selectListItems;
    //
    //
    //     return productListViewModel;
    // }
}