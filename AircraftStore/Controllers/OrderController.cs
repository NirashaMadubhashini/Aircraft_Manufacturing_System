using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Models.ViewModels;
using Aircraft.Ultitity;
using Stripe;
using Stripe.Checkout;
using System.Text;
using iTextSharp.text.pdf;
using iTextSharp.text;


namespace Aircraft.Controllers;

public class OrderController : Controller
{
    private readonly IUnitOfWork _unitOfWork;

    public OrderController(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index(string? status, string? phoneNumber)
    {
        IEnumerable<ShopOrder> orders;

        if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        {
            orders = await _unitOfWork.Orders
                .GetAllAsync(include: e =>
                    e.Include(e => e.ApplicationUser));
        }
        else
        {
            ClaimsIdentity? claimsIdentity = (ClaimsIdentity?)User.Identity;
            Claim? claim = claimsIdentity?.FindFirst(ClaimTypes.NameIdentifier);
            string? applicationUserId = claim?.Value;

            if (applicationUserId != null)
            {
                orders = await _unitOfWork.Orders
                    .GetAllAsync(e => e.ApplicationUserId == applicationUserId,
                        include: e =>
                            e.Include(e => e.ApplicationUser));
            }
            else if (phoneNumber == null)
            {
                return RedirectToAction("SearchOrder");
            }
            else
            {
                orders = await _unitOfWork.Orders.GetAllAsync(e => e.PhoneNumber == phoneNumber,
                    include: e =>
                        e.Include(e => e.ApplicationUser));
            }
        }

        switch (status)
        {
            case "pending":
                orders = orders.Where(e => e.PaymentStatus == SD.PaymentStatusDelayedPayment);
                break;
            case "inprocess":
                orders = orders.Where(e => e.OrderStatus == SD.StatusInpProcess);
                break;
            case "completed":
                orders = orders.Where(e => e.OrderStatus == SD.StatusShipped);
                break;
            case "approved":
                orders = orders.Where(e => e.OrderStatus == SD.StatusApproved);
                break;
            case "all":
                break;
        }

        return View(orders);
    }

    public IActionResult SearchOrder()
    {
        return View();
    }

    public async Task<IActionResult> Details(int id)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(e => e.Id == id,
            include: o => o.Include(e => e.ApplicationUser));
        if (orderFromDb == null)
        {
            return NotFound();
        }

        OrderViewModel orderViewModel = new OrderViewModel()
        {
            Order = orderFromDb,
            OrderDetails = await _unitOfWork.OrderDetails.GetAllAsync(e => e.OrderId == id,
                include: o => o
                    .Include(e => e.AirplaneSize)
                    .ThenInclude(e => e.AirplaneColor)
                    .ThenInclude(e => e.Airplane)
                    .Include(e => e.AirplaneSize)
                    .ThenInclude(e => e.AirplaneColor)
                    .ThenInclude(e => e.Color)!
            )
        };

        return View(orderViewModel);
    }

    [ActionName(nameof(Details))]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Details_PAY_NOW(OrderViewModel orderViewModel)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(
            e => orderViewModel.Order != null && e.Id == orderViewModel.Order.Id,
            include: o => o.Include(e => e.ApplicationUser));
        if (orderFromDb == null)
        {
            return NotFound();
        }

        orderViewModel.Order = orderFromDb;

        orderViewModel.OrderDetails = await _unitOfWork.OrderDetails.GetAllAsync(e => e.OrderId == orderFromDb.Id,
            include: o => o.Include(e => e.AirplaneSize));

        //--------
        // stripe settings
        var domain = $"{Request.Scheme}://{Request.Host.Value}";
        // var options = new SessionCreateOptions
        var options = new SessionCreateOptions
        {
            LineItems = new List<SessionLineItemOptions>(),
            Mode = "payment",
            SuccessUrl = $"{domain}/Cart/PaymentConfirmation?id={orderViewModel.Order.Id}",
            CancelUrl = $"{domain}/Order/Details?id={orderViewModel.Order.Id}",
            PaymentMethodTypes = new List<string>()
            {
                "card",
            },
        };

        foreach (var orderDetail in orderFromDb.OrderDetails)
        {
            AirplaneColor airplaneColor = (await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(
                e => e.AirplaneSizes!.Any(ss => ss.Id == orderDetail.AirplaneSizeId),
                include: o => o.Include(e => e.Airplane)
                    .Include(e => e.Images)!
                    .Include(e => e.Color)!
            ))!;

            var s = airplaneColor?.Images?.Select(e => Url.Content(e.Path)).ToList() ?? new List<string>();

            var sessionLineItem = new SessionLineItemOptions
            {
                PriceData = new SessionLineItemPriceDataOptions
                {
                    UnitAmount = (long)(orderDetail.PriceEach * 100),
                    Currency = "usd",
                    ProductData = new SessionLineItemPriceDataProductDataOptions
                    {
                        Name = $"{airplaneColor.Airplane.Name} {airplaneColor.Color?.Name}",
                        Images = airplaneColor?.Images?.Select(e => $"{domain}{e.Path}").ToList() ?? new List<string>(),
                    },
                },
                Quantity = orderDetail.Count,
            };

            options.LineItems.Add(sessionLineItem);
        }

        var service = new SessionService();
        Session session = service.Create(options);

        await _unitOfWork.Orders.UpdateStripePaymentId(orderViewModel.Order.Id, session.Id, session.PaymentIntentId);
        await _unitOfWork.SaveChangesAsync();

        Response.Headers.Add("Location", session.Url);
        return new StatusCodeResult(303);
        //--------
    }

    public async Task<IActionResult> PaymentConfirmation(int id)
    {
        ShopOrder? order = await _unitOfWork.Orders.FirstOrDefaultAsync(e => e.Id == id);

        if (order == null)
        {
            return NotFound();
        }

        if (order.PaymentStatus == SD.PaymentStatusDelayedPayment)
        {
            var service = new SessionService();
            Session session = service.Get(order.SessionId);
            // check the stripe status
            if (session.PaymentStatus.ToLower() == "paid")
            {
                await _unitOfWork.Orders.UpdateStatusAsync(id, order.OrderStatus, SD.PaymentStatusApproved);
                await _unitOfWork.SaveChangesAsync();
            }
        }

        return View(id);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> UpdateOrderDetail(OrderViewModel orderViewModel)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(
            e => orderViewModel.Order != null && e.Id == orderViewModel.Order.Id,
            include: o => o.Include(e => e.ApplicationUser));
        if (orderFromDb == null)
        {
            return NotFound();
        }

        if (orderViewModel.Order != null)
        {
            orderFromDb.Name = orderViewModel.Order.Name;
            orderFromDb.PhoneNumber = orderViewModel.Order.PhoneNumber;
            orderFromDb.Address = orderViewModel.Order.Address;
            orderFromDb.City = orderViewModel.Order.City;
            orderFromDb.District = orderViewModel.Order.District;
            orderFromDb.Ward = orderViewModel.Order.Ward;
            orderFromDb.PostalCode = orderViewModel.Order.PostalCode;

            if (orderViewModel.Order.Carrier != null)
            {
                orderFromDb.Carrier = orderViewModel.Order.Carrier;
            }

            if (orderViewModel.Order.TrackingNumber != null)
            {
                orderFromDb.TrackingNumber = orderViewModel.Order.TrackingNumber;
            }
        }

        _unitOfWork.Orders.Update(orderFromDb);
        await _unitOfWork.SaveChangesAsync();
        TempData[SD.Success] = "Order Details Updated Successfully!";

        return RedirectToAction("Details", "Order", new { id = orderFromDb.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> StartProcessing(OrderViewModel orderViewModel)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(
            e => orderViewModel.Order != null && e.Id == orderViewModel.Order.Id,
            include: o => o.Include(e => e.ApplicationUser));

        if (orderFromDb == null)
        {
            return NotFound();
        }

        await _unitOfWork.Orders.UpdateStatusAsync(orderFromDb.Id, SD.StatusInpProcess);
        await _unitOfWork.SaveChangesAsync();
        TempData[SD.Success] = "Order Status Updated Successfully!";

        return RedirectToAction("Details", "Order", new { id = orderFromDb.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> ShipOrder(OrderViewModel orderViewModel)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(
            e => orderViewModel.Order != null && e.Id == orderViewModel.Order.Id,
            include: o => o.Include(e => e.ApplicationUser));

        if (orderFromDb == null)
        {
            return NotFound();
        }

        if (orderViewModel.Order != null)
        {
            orderFromDb.ShippingDate = DateTime.Now;
            orderFromDb.OrderStatus = SD.StatusShipped;
            orderFromDb.TrackingNumber = orderViewModel.Order.TrackingNumber;
            orderFromDb.Carrier = orderViewModel.Order.Carrier;
            if (orderFromDb.PaymentStatus == SD.PaymentStatusDelayedPayment)
            {
                orderFromDb.PaymentDueDate = DateTime.Now.AddDays(30);
            }
        }

        _unitOfWork.Orders.Update(orderFromDb);
        await _unitOfWork.SaveChangesAsync();
        TempData[SD.Success] = "Order Shipped Successfully!";

        return RedirectToAction("Details", "Order", new { id = orderFromDb.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> CancelOrder(OrderViewModel orderViewModel)
    {
        ShopOrder? orderFromDb = await _unitOfWork.Orders.FirstOrDefaultAsync(
            e => orderViewModel.Order != null && e.Id == orderViewModel.Order.Id,
            include: o => o.Include(e => e.ApplicationUser));

        if (orderFromDb == null)
        {
            return NotFound();
        }

        // begin transaction
        var transaction = await _unitOfWork.BeginTransactionAsync();

        List<OrderDetail> orderDetails = await _unitOfWork.OrderDetails.GetAllAsync(e => e.OrderId == orderFromDb.Id);

        foreach (var orderDetail in orderDetails)
        {
            AirplaneSize? airplaneSize =
                await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == orderDetail.AirplaneSizeId);
            if (airplaneSize == null)
            {
                throw new Exception("AirplaneSize does not exist!");
            }

            airplaneSize.Quantity += orderDetail.Count;
            _unitOfWork.AirplaneSizes.Update(airplaneSize);
        }

        if (orderFromDb.PaymentStatus == SD.PaymentStatusApproved)
        {
            var options = new RefundCreateOptions()
            {
                Reason = RefundReasons.RequestedByCustomer,
                PaymentIntent = orderFromDb.PaymentIntentId,
            };

            var service = new RefundService();
            Refund refund = await service.CreateAsync(options);

            await _unitOfWork.Orders.UpdateStatusAsync(orderFromDb.Id, SD.StatusCancelled, SD.StatusRefunded);
        }
        else
        {
            await _unitOfWork.Orders.UpdateStatusAsync(orderFromDb.Id, SD.StatusCancelled, SD.StatusCancelled);
        }

        await _unitOfWork.SaveChangesAsync();

        await transaction.CommitAsync();
        // end Transaction

        TempData[SD.Success] = "Order Canceled Successfully!";

        return RedirectToAction("Details", "Order", new { id = orderFromDb.Id });
    }

    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> GenerateReport(int id)
    {
        var order = await _unitOfWork.Orders.FirstOrDefaultAsync(e => e.Id == id,
            include: o => o.Include(e => e.ApplicationUser)
                          .Include(e => e.OrderDetails)
                              .ThenInclude(d => d.AirplaneSize)
                              .ThenInclude(s => s.AirplaneColor)
                              .ThenInclude(c => c.Airplane)
                              .Include(e => e.OrderDetails)
                              .ThenInclude(d => d.AirplaneSize)
                              .ThenInclude(s => s.AirplaneColor)
                              .ThenInclude(c => c.Color));

        if (order == null)
        {
            return NotFound();
        }

        using (MemoryStream memoryStream = new MemoryStream())
        {
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            PdfPTable table = new PdfPTable(5); // 5 columns
            PdfPCell cell = new PdfPCell(new Phrase("Order Report"))
            {
                Colspan = 5,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 10,
                BackgroundColor = new BaseColor(31, 104, 149)
            };

            table.AddCell(cell);
            table.AddCell("OrderId");
            table.AddCell("ProductName");
            table.AddCell("Quantity");
            table.AddCell("PriceEach");
            table.AddCell("TotalPrice");

            foreach (var detail in order.OrderDetails)
            {
                var airplaneColor = detail.AirplaneSize?.AirplaneColor;
                var colorName = airplaneColor?.Color?.Name ?? "Unknown Color";
                string productName = $"{airplaneColor?.Airplane?.Name ?? "Unknown Airplane"} {colorName}";

                table.AddCell(order.Id.ToString());
                table.AddCell(productName);
                table.AddCell(detail.Count.ToString());
                table.AddCell(detail.PriceEach.ToString());
                table.AddCell((detail.Count * detail.PriceEach).ToString());
            }

            document.Add(table);
            document.Close();

            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();
            return File(bytes, "application/pdf", $"Order_{id}_Report.pdf");
        }
    }

    [Authorize(Roles = $"{SD.Role_Admin},{SD.Role_Employee}")]
    public async Task<IActionResult> GenerateAllOrdersReport()
    {
        var allOrders = await _unitOfWork.Orders
            .GetAllAsync(include: o => o.Include(e => e.ApplicationUser));

        using (MemoryStream memoryStream = new MemoryStream())
        {
            Document document = new Document(PageSize.A4, 10, 10, 10, 10);
            PdfWriter writer = PdfWriter.GetInstance(document, memoryStream);
            document.Open();

            PdfPTable table = new PdfPTable(6); // 6 columns
            PdfPCell cell = new PdfPCell(new Phrase("All Orders Report"))
            {
                Colspan = 6,
                HorizontalAlignment = Element.ALIGN_CENTER,
                Padding = 10,
                BackgroundColor = new BaseColor(53, 137, 213)
            };

            table.AddCell(cell);
            table.AddCell("OrderId");
            table.AddCell("Customer Name");
            table.AddCell("Phone Number");
            table.AddCell("Email");
            table.AddCell("Status");
            table.AddCell("Total");

            foreach (var order in allOrders)
            {
                table.AddCell(order.Id.ToString());
                table.AddCell(order.Name);
                table.AddCell(order.PhoneNumber);
                table.AddCell(order.ApplicationUser?.Email ?? "N/A");
                table.AddCell(order.OrderStatus);
                table.AddCell(order.OrderTotal.ToString("C"));
            }

            document.Add(table);
            document.Close();

            byte[] bytes = memoryStream.ToArray();
            memoryStream.Close();
            return File(bytes, "application/pdf", "All_Orders_Report.pdf");
        }
    }

}