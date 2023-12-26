/*using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using Aircraft.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Infrastructure;
using Aircraft.Models;
using Aircraft.Models.ViewModels;
using Aircraft.Ultitity;

namespace Aircraft.Pages;

public class CartModel : PageModel
{
    private readonly IUnitOfWork _unitOfWork;
    public Cart Cart { get; set; }
    public string ReturnUrl { get; set; } = "/";
    [BindProperty] public CartViewModel CartViewModel { get; set; }

    public CartModel(Cart cartService, IUnitOfWork unitOfWork)
    {
        Cart = cartService;
        _unitOfWork = unitOfWork;
    }


    public async Task OnGet(string? returnUrl)
    {
        ReturnUrl = returnUrl ?? "/";

        CartViewModel = new CartViewModel()
        {
            ReturnUrl = ReturnUrl
        };

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        var cartItems = await _unitOfWork.CartItems.GetAllAsync(e => e.ApplicationUserId == applicationUserId);

        foreach (var cartItem in cartItems)
        {
            AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes
                .FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                    include: o => o.Include(e => e.Size)
                        .Include(e => e.AirplaneColor)
                        .ThenInclude(e => e.Images)
                        .Include(e => e.AirplaneColor)
                        .ThenInclude(e => e.Airplane)
                        .ThenInclude(e => e.Brand));

            if (airplaneSize != null)
            {
                CartViewModel.ProductCartsList.Add(
                    new ProductCartViewModel()
                    {
                        AirplaneSizeId = airplaneSize.Id,
                        ProductName =
                            $"{airplaneSize.AirplaneColor.Airplane.Name}  (size: {airplaneSize.Size.Unit} {airplaneSize.Size.Value})",
                        BrandName = airplaneSize.AirplaneColor.Airplane.Brand.Name,
                        Price = airplaneSize.AirplaneColor.SalePrice,
                        Quantity = cartItem.Count,
                        ImgPath = airplaneSize.AirplaneColor.Images?.First().Path,
                        ProductUrl = airplaneSize.AirplaneColor.Url
                    }
                );
            }
        }
    }
    // public async Task OnGet(string? returnUrl)
    // {
    //     ReturnUrl = returnUrl ?? "/";
    //     // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
    //     CartViewModel = new CartViewModel()
    //     {
    //         ReturnUrl = ReturnUrl
    //     };
    //     
    //     foreach (var line in Cart.CartItemsList)
    //     {
    //         AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes
    //             .FirstOrDefaultAsync(e => e.Id == line.AirplaneSizeId,
    //                 include: o => o.Include(e => e.Size)
    //                     .Include(e => e.AirplaneColor)
    //                     .ThenInclude(e => e.Images)
    //                     .Include(e => e.AirplaneColor)
    //                     .ThenInclude(e => e.Airplane)
    //                     .ThenInclude(e => e.Brand));
    //
    //         if (airplaneSize != null)
    //         {
    //             CartViewModel.ProductCartsList.Add(
    //                 new ProductCartViewModel()
    //                 {
    //                     AirplaneSizeId = airplaneSize.Id,
    //                     ProductName =
    //                         $"{airplaneSize.AirplaneColor.Airplane.Name}  (size: {airplaneSize.Size.Unit} {airplaneSize.Size.Value})",
    //                     BrandName = airplaneSize.AirplaneColor.Airplane.Brand.Name,
    //                     Price = airplaneSize.AirplaneColor.SalePrice,
    //                     Quantity = line.Count,
    //                     ImgPath = airplaneSize.AirplaneColor.Images?.First().Path,
    //                     ProductUrl = airplaneSize.AirplaneColor.Url
    //                 }
    //             );
    //         }
    //     }
    // }

    public async Task<IActionResult> OnPost(CartItem cartItem, string returnUrl)
    {
        if (ModelState.IsValid)
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
            Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            string? applicationUserId = claim?.Value;
        
            cartItem.ApplicationUserId = applicationUserId;

            AirplaneSize? airplaneSize = await _unitOfWork.AirplanehoeSizes
                .FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId);

            if (airplaneSize != null)
            {
                CartItem? cartItemFromDb = (await _unitOfWork.CartItems.FirstOrDefaultAsync(e =>
                    e.ApplicationUserId == applicationUserId && e.AirplaneSizeId == cartItem.AirplaneSizeId));

                if (cartItemFromDb != null)
                {
                    cartItem.Id = cartItemFromDb.Id;
                
                    cartItem.Count += cartItemFromDb.Count;
                    if (cartItem.Count > airplaneSize.Quantity)
                    {
                        cartItem.Count = airplaneSize.Quantity;
                    }

                    _unitOfWork.CartItems.Update(cartItem);
                }
                else
                {
                    await _unitOfWork.CartItems.AddAsync(cartItem);
                }
            
                await _unitOfWork.SaveChangesAsync();
            }

            TempData[SD.Success] = "Added to Cart";
            return RedirectToPage(new { returnUrl });
        }
        
        TempData[SD.Error] = "Some things is wrong!";

        return Redirect(returnUrl);
    }
    // public async Task<IActionResult> OnPost(int AirplaneSizeId, string returnUrl, int Count = 1)
    // {
    //     AirplaneSize? airplaneSize = await _context.AirplaneSize
    //         .FirstOrDefaultAsync(e => e.Id == AirplaneSizeId);
    //
    //     if (airplaneSize != null)
    //     {
    //         // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
    //         Cart.AddItem(AirplaneSizeId, Count > 0 ? Count : 1);
    //         CartItem? line = Cart.CartItemsList
    //             .FirstOrDefault(p => p.AirplaneSizeId == AirplaneSizeId);
    //         if (line.Count > airplaneSize.Quantity)
    //         {
    //             line.Count = airplaneSize.Quantity;
    //         }
    //
    //         HttpContext.Session.SetJson("cart", Cart);
    //     }
    //
    //     return RedirectToPage(new { returnUrl = returnUrl });
    // }

    public async Task<IActionResult> OnPostSubtractAsync(int productId, string returnUrl)
    {
        // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        Cart.SubtractItem(productId, 1);
        HttpContext.Session.SetJson("cart", Cart);

        return RedirectToPage(new { returnUrl = returnUrl });
    }

    public async Task<IActionResult> OnPostDeleteAsync(int productId, string returnUrl)
    {
        // Cart = HttpContext.Session.GetJson<Cart>("cart") ?? new Cart();
        Cart.RemoveLine(productId);
        HttpContext.Session.SetJson("cart", Cart);

        return RedirectToPage(new { returnUrl = returnUrl });
    }
}*/