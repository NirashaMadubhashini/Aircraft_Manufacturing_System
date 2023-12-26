using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Models.ViewModels;
using Aircraft.Ultitity;

namespace Aircraft.Controllers;

public class ProductsController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public int PageSize = 9;
    public Cart Cart { get; set; }

    // GET
    public ProductsController(IUnitOfWork unitOfWork, Cart cart)
    {
        _unitOfWork = unitOfWork;
        Cart = cart;
    }

    public async Task<IActionResult> Index(string? productName, int page = 1, string? brand = null, int? size = null,
        decimal? minPrice = 0,
        decimal? maxPrice = 999, string? sort = "latest")
    {
        ProductListViewModel productListViewModel = await _unitOfWork.AirplaneColors.FilterProductAsync(PageSize,
            productName, page, brand, size, minPrice, maxPrice,
            sort);
        return View(productListViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> Detail(string url)
    {
        AirplaneColor? airplaneColor = await _unitOfWork.AirplaneColors
                .FirstOrDefaultAsync(e => e.Url == url,
                    include: o => o
                        .Include(e => e.Images)
                        .Include(e => e.AirplaneSizes)!
                        .ThenInclude(e => e.Size)!
                )
            ;
        if (airplaneColor == null)
        {
            return NotFound();
        }

        Airplane airplane = await _unitOfWork.Airplanes
            .FirstOrDefaultAsync(e => e.AirplaneColors!.Any(e => e.Id == airplaneColor.Id),
                include: o => o
                    .Include(e => e.Brand)
                    .Include(e => e.AirplaneColors)!
                    .ThenInclude(e => e.Color)
                    .Include(e => e.AirplaneColors)!
                    .ThenInclude(e => e.Images)!
            );

        List<AirplaneColor> relatedAirplaneColors = airplane.AirplaneColors!.OrderBy(e => e.SortOrder).ToList();

        IEnumerable<AirplaneColor> relatedAirplanes = await _unitOfWork.AirplaneColors
            .GetAllAsync(e => e.Airplane.Brand.Id == airplane.Brand.Id && e.Id != airplaneColor.Id,
                include: o => o
                    .Include(e => e.Images)
                    .Include(e => e.Airplane.Brand),
                orderBy: e => e.Priority,
                take: 4
            );

        ProductDetailViewModel productDetailViewModel = new ProductDetailViewModel
        {
            Airplane = airplane,
            AirplaneColor = airplaneColor,
            AirplaneImages = airplaneColor.Images,
            AirplaneSizes = airplaneColor.AirplaneSizes,
            RelatedProduct = relatedAirplanes,
            RelatedAirplaneColors = relatedAirplaneColors
        };
        return View(productDetailViewModel);
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult>
        AddToCart(CartItem cartItem, string? returnUrl)
    {
        returnUrl ??= "/";

        if (ModelState.IsValid)
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
            Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            string? applicationUserId = claim?.Value;

            if (applicationUserId != null)
            {
                cartItem.ApplicationUserId = applicationUserId;

                AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes
                    .FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                        include: e => e.Include(e => e.AirplaneColor));

                if (airplaneSize == null)
                {
                    return NotFound();
                }
                
                if (airplaneSize.Quantity == 0)
                {
                    TempData[SD.Error] = "This size is out of stock!";
                    return Redirect(returnUrl);
                }

                if (airplaneSize.Quantity < cartItem.Count)
                {
                    TempData[SD.Error] = $"There're only {airplaneSize.Quantity} items of this size left";
                    return Redirect(returnUrl);
                }

                cartItem.PriceEach = airplaneSize.AirplaneColor.SalePrice;

                CartItem? cartItemFromDb = (await _unitOfWork.CartItems.FirstOrDefaultAsync(e =>
                    e.ApplicationUserId == applicationUserId && e.AirplaneSizeId == cartItem.AirplaneSizeId));

                if (cartItemFromDb == null)
                {
                    await _unitOfWork.CartItems.AddAsync(cartItem);
                }
                else
                {
                    cartItem.Id = cartItemFromDb.Id;

                    int newCount = cartItem.Count + cartItemFromDb.Count;
                    
                    if (newCount > airplaneSize.Quantity)
                    {
                        int diff = airplaneSize.Quantity - cartItemFromDb.Count;

                        TempData[SD.Error] = diff == 0
                            ? "Can't add more items of this size to Cart!"
                            : $"You can only add {diff} more items of this size to Cart";

                        return Redirect(returnUrl);
                    }

                    cartItem.Count = newCount;
                    _unitOfWork.CartItems.Update(cartItem);
                }

                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction("Index", "Cart", new { returnUrl });
            }
        }

        TempData[SD.Error] = "Did you forgot to chose size?";

        return Redirect(returnUrl);
    }

    [HttpPost]
    [AllowAnonymous]
    public async Task<IActionResult>
        AddToCartSession(CartItem cartItem, string? returnUrl)
    {
        returnUrl ??= "/";

        if (ModelState.IsValid)
        {
            AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes
                .FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                    include: e => e.Include(e => e.AirplaneColor));

            if (airplaneSize == null)
            {
                return NotFound();
            }

            if (airplaneSize.Quantity == 0)
            {
                TempData[SD.Error] = "This size is out of stock!";
                return Redirect(returnUrl);
            }

            if (airplaneSize.Quantity < cartItem.Count)
            {
                TempData[SD.Error] = $"There're only {airplaneSize.Quantity} items of this size left!";
                return Redirect(returnUrl);
            }

            cartItem.PriceEach = airplaneSize.AirplaneColor.SalePrice;

            CartItem? cartItemFromSession = Cart.CartItemsList.FirstOrDefault(e => e.AirplaneSizeId == cartItem.AirplaneSizeId);

            if (cartItemFromSession == null)
            {
                Cart.AddItem(airplaneSize.Id, cartItem.Count);
            }
            else
            {
                cartItem.Id = cartItemFromSession.Id;

                int newCount = cartItem.Count + cartItemFromSession.Count;

                if (newCount > airplaneSize.Quantity)
                {
                    int diff = airplaneSize.Quantity - cartItemFromSession.Count;

                    TempData[SD.Error] = diff == 0
                        ? "Can't add more items of this size to Cart!"
                        : $"You can only add {diff} more items of this size to Cart";

                    return Redirect(returnUrl);
                }

                Cart.AddItem(airplaneSize.Id, cartItem.Count);
            }
            return RedirectToAction("Index", "Cart", new { returnUrl });
        }

        TempData[SD.Error] = "Did you forgot to chose size?";

        return Redirect(returnUrl);
    }
}