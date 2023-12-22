﻿using Aircraft.Models;
using Aircraft.Models.ViewModels;

namespace Aircraft.DataAccess.Repository.IRepository;

public interface IShoeColorRepository : IRepository<ShoeColor>
{
    void Update(ShoeColor obj);

    Task<ProductListViewModel> FilterProductAsync(
        int pageSize,
        string? productName,
        int page = 1,
        string? brand = null,
        int? size = null,
        decimal? minPrice = 0,
        decimal? maxPrice = 999, string? sort = "latest");
}