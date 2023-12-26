using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Models.ViewModels;
using Aircraft.Ultitity;

namespace Aircraft.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly IUnitOfWork _unitOfWork;

    public HomeController(ILogger<HomeController> logger, IUnitOfWork unitOfWork)
    {
        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<IActionResult> Index()
    {
        // if (User.IsInRole(SD.Role_Admin) || User.IsInRole(SD.Role_Employee))
        // {
        //     return RedirectToAction("Index", "Admin");
        // }
        
        Brand? A380800 = await _unitOfWork.Brands.FirstOrDefaultAsync(e => e.Name == "A380800");
        Brand? A350900XWB = await _unitOfWork.Brands.FirstOrDefaultAsync(e => e.Name == "A350900XWB");
        Brand? LimitedEdition = await _unitOfWork.Brands.FirstOrDefaultAsync(e => e.Name == "LimitedEdition");
        Brand? Boeing777 = await _unitOfWork.Brands.FirstOrDefaultAsync(e => e.Name == "Boeing777");
        
        HomeViewModel homeViewModel = new HomeViewModel()
        {
            FeatureProducts = await _unitOfWork.AirplaneColors
                .GetAllAsync(include: o => o
                        .Include(e => e.Images)
                        .Include(e => e.Airplane)
                        .ThenInclude(e => e!.Brand)!,
                    orderBy: e => e.Priority,
                    take: 4
                ),
            A380800Products = await _unitOfWork.AirplaneColors
                .GetAllAsync(e => A380800 != null && e.Airplane!.BrandId == A380800.Id,
                    include: o => o
                    .Include(e => e.Images)
                    .Include(e => e.Airplane)
                    .ThenInclude(e => e!.Brand)!,
                    orderBy: e => e.Priority,
                    take: 4
                ),
            A350900XWBProducts = await _unitOfWork.AirplaneColors
                .GetAllAsync(e => A350900XWB != null && e.Airplane!.BrandId == A350900XWB.Id,
                    include: o => o
                        .Include(e => e.Images)
                        .Include(e => e.Airplane)
                        .ThenInclude(e => e!.Brand)!,
                    orderBy: e => e.Priority,
                    take: 4
                ),
            LimitedEditionProducts = await _unitOfWork.AirplaneColors
                .GetAllAsync(e => LimitedEdition != null && e.Airplane!.BrandId == LimitedEdition.Id,
                    include: o => o
                        .Include(e => e.Images)
                        .Include(e => e.Airplane)
                        .ThenInclude(e => e!.Brand)!,
                    orderBy: e => e.Priority,
                    take: 4
                ),
            Boeing777Products = await _unitOfWork.AirplaneColors
                .GetAllAsync(e => Boeing777 != null && e.Airplane!.BrandId == Boeing777.Id,
                    include: o => o
                        .Include(e => e.Images)
                        .Include(e => e.Airplane)
                        .ThenInclude(e => e!.Brand)!,
                    orderBy: e => e.Priority,
                    take: 4
                ),
        };
        return View(homeViewModel);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}