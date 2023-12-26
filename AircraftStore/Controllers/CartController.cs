using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Models.ViewModels;
using Aircraft.Ultitity;
using Stripe.Checkout;

namespace Aircraft.Controllers;

public class CartController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public Cart Cart { get; set; }

    public CartController(IUnitOfWork unitOfWork, Cart cart)
    {
        _unitOfWork = unitOfWork;
        Cart = cart;
    }

    public async Task<IActionResult> Index(string? returnUrl)
    {
        CartViewModel cartViewModel = new CartViewModel()
        {
            ReturnUrl = returnUrl ?? "/",
        };

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        List<CartItem> cartItemList;

        if (applicationUserId != null)
        {
            cartItemList = await _unitOfWork.CartItems.GetAllAsync(e => e.ApplicationUserId == applicationUserId);
        }
        else
        {
            cartItemList = Cart.CartItemsList;
        }

        foreach (var cartItem in cartItemList)
        {
            AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                include: o => o.Include(e => e.Size)
                    .Include(e => e.AirplaneColor)
                    .ThenInclude(e => e.Images)
                    .Include(e => e.AirplaneColor)
                    .ThenInclude(e => e.Airplane)
                    .ThenInclude(e => e.Brand));

            if (airplaneSize != null)
            {
                cartViewModel.ShopOrder.OrderTotal += airplaneSize.AirplaneColor.SalePrice * cartItem.Count;

                cartViewModel.ProductCartsList.Add(
                    new ProductCartViewModel()
                    {
                        AirplaneSizeId = airplaneSize.Id,
                        CartItemId = cartItem.Id,
                        ProductName =
                            $"{airplaneSize.AirplaneColor.Airplane.Name}",
                        Size = $"{airplaneSize.Size?.Unit} {airplaneSize.Size?.Value}",
                        BrandName = airplaneSize.AirplaneColor.Airplane?.Brand.Name,
                        Price = airplaneSize.AirplaneColor.SalePrice,
                        Quantity = cartItem.Count,
                        ImgPath = airplaneSize.AirplaneColor.Images?.First().Path,
                        ProductUrl = airplaneSize.AirplaneColor.Url!
                    }
                );
            }
        }

        return View(cartViewModel);
    }

    public async Task<IActionResult> Increment(int airplaneSizeId, string? returnUrl)
    {
        AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == airplaneSizeId);

        if (airplaneSize == null)
        {
            return NotFound();
        }

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        CartItem? cartItem;

        if (applicationUserId != null)
        {
            cartItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(e =>
                e.ApplicationUserId == applicationUserId && e.AirplaneSizeId == airplaneSizeId);
        }
        else
        {
            cartItem = Cart.CartItemsList.FirstOrDefault(e => e.AirplaneSizeId == airplaneSizeId);
        }

        if (cartItem == null || cartItem.Count < 1)
        {
            return NotFound();
        }


        if (cartItem.Count >= airplaneSize.Quantity)
        {
            TempData[SD.Error] = "Can't add more item of this product!";
            return RedirectToAction("Index", new { returnUrl });
        }


        if (applicationUserId != null)
        {
            cartItem.Count++;
            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            Cart.AddItem(airplaneSizeId, 1);
        }

        TempData[SD.Success] = "Increase an item's quantity by 1!";

        return RedirectToAction("Index", new { returnUrl });
    }

    public async Task<IActionResult> Decrement(int airplaneSizeId, string? returnUrl)
    {
        AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == airplaneSizeId);
        if (airplaneSize == null)
        {
            return NotFound();
        }

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        CartItem? cartItem;

        if (applicationUserId != null)
        {
            cartItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(e =>
                e.ApplicationUserId == applicationUserId && e.AirplaneSizeId == airplaneSizeId);
        }
        else
        {
            cartItem = Cart.CartItemsList.FirstOrDefault(e => e.AirplaneSizeId == airplaneSizeId);
        }

        if (cartItem == null || cartItem.Count <= 0)
        {
            return NotFound();
        }

        if (cartItem.Count <= 1)
        {
            TempData[SD.Error] = "Please use remove button instead!";
            return RedirectToAction("Index", new { returnUrl });
        }

        if (applicationUserId != null)
        {
            cartItem.Count--;
            _unitOfWork.CartItems.Update(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            Cart.SubtractItem(airplaneSizeId, 1);
        }

        TempData[SD.Success] = "Decreased 1 item's quantity!";
        return RedirectToAction("Index", new { returnUrl });
    }

    public async Task<IActionResult> Remove(int airplaneSizeId, string? returnUrl)
    {
        AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == airplaneSizeId);
        if (airplaneSize == null)
        {
            return NotFound();
        }

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        CartItem? cartItem;

        if (applicationUserId != null)
        {
            cartItem = await _unitOfWork.CartItems.FirstOrDefaultAsync(e =>
                e.ApplicationUserId == applicationUserId && e.AirplaneSizeId == airplaneSizeId);
        }
        else
        {
            cartItem = Cart.CartItemsList.FirstOrDefault(e => e.AirplaneSizeId == airplaneSizeId);
        }

        if (cartItem == null)
        {
            return NotFound();
        }

        if (applicationUserId != null)
        {
            _unitOfWork.CartItems.Remove(cartItem);
            await _unitOfWork.SaveChangesAsync();
        }
        else
        {
            Cart.RemoveLine(airplaneSizeId);
        }

        TempData[SD.Success] = "Removed 1 item from cart!";
        return RedirectToAction("Index", new { returnUrl });
    }

    public async Task<IActionResult> Summary(string? returnUrl)
    {
        returnUrl ??= "/";
        CartViewModel cartViewModel = new CartViewModel()
        {
            ReturnUrl = returnUrl ?? "/",
        };

        ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
        Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
        string? applicationUserId = claim?.Value;

        List<CartItem> cartItemList;
        if (applicationUserId != null)
        {
            cartItemList = await _unitOfWork.CartItems.GetAllAsync(e => e.ApplicationUserId == applicationUserId);
        }
        else
        {
            cartItemList = Cart.CartItemsList;
        }

        if (cartItemList.Count <= 0)
        {
            TempData[SD.Error] = "You must add somethings to Cart first!";
            return Redirect(returnUrl!);
        }

        foreach (var cartItem in cartItemList)
        {
            AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                include: o => o.Include(e => e.Size)
                    .Include(e => e.AirplaneColor)
                    .ThenInclude(e => e.Images)
                    .Include(e => e.AirplaneColor)
                    .ThenInclude(e => e.Airplane)
                    .ThenInclude(e => e.Brand));

            if (airplaneSize != null)
            {
                if (airplaneSize.Quantity < cartItem.Count || cartItem.Count <= 0 || airplaneSize.Quantity <= 0)
                {
                    return BadRequest();
                }

                cartViewModel.ShopOrder.OrderTotal += airplaneSize.AirplaneColor.SalePrice * cartItem.Count;

                cartViewModel.ProductCartsList.Add(
                    new ProductCartViewModel()
                    {
                        AirplaneSizeId = airplaneSize.Id,
                        CartItemId = cartItem.Id,
                        ProductName =
                            $"{airplaneSize.AirplaneColor.Airplane!.Name}",
                        Size = $"{airplaneSize.Size?.Unit} {airplaneSize.Size?.Value}",
                        BrandName = airplaneSize.AirplaneColor.Airplane!.Brand!.Name,
                        Price = airplaneSize.AirplaneColor.SalePrice,
                        Quantity = cartItem.Count,
                        ImgPath = airplaneSize.AirplaneColor.Images?.First().Path,
                        ProductUrl = airplaneSize.AirplaneColor.Url!
                    }
                );
            }
        }

        if (applicationUserId != null)
        {
            ApplicationUser? applicationUser =
                await _unitOfWork.ApplicationUsers.FirstOrDefaultAsync(e => e.Id == claim.Value);

            cartViewModel.ShopOrder.ApplicationUserId = applicationUserId;

            cartViewModel.ShopOrder.Name = applicationUser.Name;
            cartViewModel.ShopOrder.PhoneNumber = applicationUser.PhoneNumber;
            cartViewModel.ShopOrder.Address = applicationUser.Address;
            cartViewModel.ShopOrder.City = applicationUser.City;
            cartViewModel.ShopOrder.District = applicationUser.District;
            cartViewModel.ShopOrder.Ward = applicationUser.Ward;
            cartViewModel.ShopOrder.PostalCode = applicationUser.PostalCode;
        }


        return View(cartViewModel);
    }

    [HttpPost]
    [ActionName(nameof(Summary))]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SummaryPost(CartViewModel cartViewModel)
    {
        cartViewModel.ShopOrder.OrderTotal = 0;

        if (ModelState.IsValid)
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
            Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            string? applicationUserId = claim?.Value;

            List<CartItem> cartItemList;
            if (applicationUserId != null)
            {
                cartItemList = await _unitOfWork.CartItems.GetAllAsync(e => e.ApplicationUserId == applicationUserId);
                cartViewModel.ShopOrder.ApplicationUserId = applicationUserId;
            }
            else
            {
                cartItemList = Cart.CartItemsList;
            }

            if (cartItemList.Count <= 0)
            {
                TempData[SD.Error] = "You must add somethings to Cart first!";
                return RedirectToAction("Index", "Home");
            }

            cartViewModel.ShopOrder.PaymentStatus = SD.PaymentStatusPending;
            cartViewModel.ShopOrder.OrderStatus = SD.StatusPending;
            cartViewModel.ShopOrder.OrderDate = DateTime.Now;

            foreach (var cartItem in cartItemList)
            {
                AirplaneSize? airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId,
                    include: e => e.Include(e => e.AirplaneColor));

                if (airplaneSize != null)
                {
                    if (airplaneSize.Quantity < cartItem.Count || cartItem.Count <= 0 || airplaneSize.Quantity <= 0)
                    {
                        return BadRequest();
                    }

                    cartItem.PriceEach = airplaneSize.AirplaneColor.SalePrice;

                    cartViewModel.ShopOrder.OrderTotal += airplaneSize.AirplaneColor.SalePrice * cartItem.Count;
                }
            }

            // start Transaction
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                await _unitOfWork.Orders.AddAsync(cartViewModel.ShopOrder);
                await _unitOfWork.SaveChangesAsync();

                foreach (var cartItem in cartItemList)
                {
                    AirplaneSize? airplaneSize =
                        (await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == cartItem.AirplaneSizeId));
                    if (airplaneSize == null)
                    {
                        throw new Exception("AirplaneSize does not exist!");
                    }

                    airplaneSize.Quantity -= cartItem.Count;
                    _unitOfWork.AirplaneSizes.Update(airplaneSize);

                    OrderDetail orderDetail = new OrderDetail
                    {
                        AirplaneSizeId = cartItem.AirplaneSizeId,
                        Count = cartItem.Count,
                        PriceEach = cartItem.PriceEach,
                        OrderId = cartViewModel.ShopOrder.Id
                    };
                    await _unitOfWork.OrderDetails.AddAsync(orderDetail);
                    await _unitOfWork.SaveChangesAsync();
                }

                // commit Transaction
                await transaction.CommitAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                // TempData[SD.Error] = e.Message;
                TempData[SD.Error] = "Somethings wrong happen.\nCheckout Failed!";
                return RedirectToAction("Index", "Home");
            }

            // stripe settings
            var domain = $"{Request.Scheme}://{Request.Host.Value}";
            var options = new SessionCreateOptions
            {
                PaymentIntentData = new SessionPaymentIntentDataOptions
                {
                    SetupFutureUsage = "off_session"
                },
                SuccessUrl = $"{domain}/Cart/OrderConfirmation?id={cartViewModel.ShopOrder.Id}",
                CancelUrl = $"{domain}/Cart/OrderConfirmation?id={cartViewModel.ShopOrder.Id}",
                // CancelUrl = $"{domain}/Cart/Index",
                LineItems = new List<SessionLineItemOptions>(),
                Mode = "payment",
                PaymentMethodTypes = new List<string>()
                {
                    "card",
                },
            };

            foreach (var cartItem in cartItemList)
            {
                AirplaneColor airplaneColor = (await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(
                    e => e.AirplaneSizes!.Any(ss => ss.Id == cartItem.AirplaneSizeId),
                    include: o => o.Include(e => e.Airplane)
                        .Include(e => e.Images)
                        .Include(e => e.Color)!
                ))!;

                var sessionLineItem = new SessionLineItemOptions
                {
                    PriceData = new SessionLineItemPriceDataOptions
                    {
                        UnitAmount = (long)(cartItem.PriceEach * 100),
                        Currency = "usd",
                        ProductData = new SessionLineItemPriceDataProductDataOptions
                        {
                            Name = $"{airplaneColor.Airplane!.Name} {airplaneColor.Color?.Name}",
                            Images = airplaneColor.Images?.Select(e => $"{domain}{e.Path}").ToList() ?? new List<string>(),
                        },
                    },
                    Quantity = cartItem.Count,
                };

                options.LineItems.Add(sessionLineItem);
            }

            var service = new SessionService();
            Session session = service.Create(options);

            await _unitOfWork.Orders.UpdateStripePaymentId(cartViewModel.ShopOrder.Id, session.Id,
                session.PaymentIntentId);
            await _unitOfWork.SaveChangesAsync();

            // clear Cart
            if (applicationUserId != null)
            {
                _unitOfWork.CartItems.RemoveRange(cartItemList);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                Cart.Clear();
            }

            Response.Headers.Add("Location", session.Url);
            return new StatusCodeResult(303);
        }

        TempData[SD.Error] = "Please enter all field!";
        return View(cartViewModel);
    }

    public async Task<IActionResult> OrderConfirmation(int id)
    {
        ShopOrder? order = await _unitOfWork.Orders.FirstOrDefaultAsync(e => e.Id == id);

        if (order.PaymentStatus != SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(order.SessionId);
            // check the stripe status
            if (session.PaymentStatus.ToLower() == "paid")
            {
                await _unitOfWork.Orders.UpdateStatusAsync(id, SD.StatusApproved, SD.PaymentStatusApproved);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return View(id);
    }
}