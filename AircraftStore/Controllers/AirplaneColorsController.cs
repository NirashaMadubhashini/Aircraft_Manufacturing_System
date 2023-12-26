using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Ultitity;

namespace Aircraft.Controllers
{
    [Authorize(Roles = SD.Role_Admin)]
    public class AirplaneColorsController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public AirplaneColorsController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _unitOfWork.AirplaneColors.GetAllAsync(include: e => e.Include(s => s.Color)
                .Include(s => s.Airplane)!);
            return View(await applicationDbContext);
        }

        // GET: AirplaneColors/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id,
                include: e => e.Include(s => s.Images)
                    .Include(s => s.AirplaneSizes)!
                    .ThenInclude(e => e.Size));

            if (airplaneColor == null)
            {
                return NotFound();
            }

            ViewData["ColorId"] =
                new SelectList(new[] { await _unitOfWork.Colors.FirstOrDefaultAsync(e => e.Id == airplaneColor.ColorId) },
                    "Id", "Name", airplaneColor.ColorId);
            ViewData["AirplaneId"] =
                new SelectList(new[] { await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == airplaneColor.AirplaneId) },
                    "Id", "Name", airplaneColor.AirplaneId);
            // ViewBag.SizeId = new SelectList(_context.Sizes, "Id", "Value");
            ViewBag.Sizes = await _unitOfWork.Sizes.GetAllAsync();

            return View(airplaneColor);
        }

        // GET: AirplaneColors/Create
        public async Task<IActionResult> Create(int airplaneId)
        {
            Airplane? airplane = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == airplaneId);
            if (airplane == null)
            {
                return NotFound();
            }

            ViewData["ColorId"] =
                new SelectList(await _unitOfWork.Colors.GetAllAsync(orderBy: e => e.Name), "Id", "Name");
            ViewData["AirplaneId"] = new SelectList(new[] { airplane }, "Id", "Name", airplaneId.ToString());
            // ViewData["AirplaneName"] = airplane.Name;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            // [Bind("Id,FactoryPrice,SalePrice,SortOrder,Active,AirplaneId,ColorId")]
            AirplaneColor airplaneColor, IFormFileCollection? formFileCollection)
        {
            if (ModelState.IsValid)
            {
                Airplane? airplaneFromDb = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == airplaneColor.AirplaneId);
                Color? colorFromDb = await _unitOfWork.Colors.FirstOrDefaultAsync(e => e.Id == airplaneColor.ColorId);
                if (airplaneFromDb == null || colorFromDb == null)
                {
                    return NotFound();
                }

                airplaneColor.Url = Regex.Replace(airplaneFromDb.Name.ToLower(), @"\W+", "-")
                                + "-"
                                + Regex.Replace(colorFromDb.Name.ToLower(), @"\W+", "-");

                airplaneColor.Created = DateTime.Now;
                airplaneColor.Edited = DateTime.Now;
                await _unitOfWork.AirplaneColors.AddAsync(airplaneColor);
                await _unitOfWork.SaveChangesAsync();

                if (formFileCollection != null)
                {
                    int count = 0;
                    // var imageUrls = new List<string>();
                    string root = _webHostEnvironment.WebRootPath;

                    foreach (var formFile in formFileCollection)
                    {
                        string fileName = Guid.NewGuid().ToString();
                        string fileExtension = Path.GetExtension(formFile.FileName);
                        string filePath = Path.Combine(root, @"images\airplanes", fileName + fileExtension);
                        await using (var fileStream = new FileStream(filePath, FileMode.Create))
                        {
                            await formFile.CopyToAsync(fileStream);
                        }

                        // imageUrls.Add(@"\images\airplanes\" + fileName + fileExtension);
                        var image = new Image
                        {
                            AirplaneColorId = airplaneColor.Id,
                            Path = @"\images\airplanes\" + fileName + fileExtension,
                            SortOrder = ++count
                        };
                        await _unitOfWork.Images.AddAsync(image);
                    }

                    await _unitOfWork.SaveChangesAsync();
                }

                return RedirectToAction(nameof(Edit), new { id = airplaneColor.Id });
            }

            ViewData["ColorId"] = new SelectList(await _unitOfWork.Colors.GetAllAsync(orderBy: e => e.Name), "Id",
                "Name", airplaneColor.ColorId);
            ViewData["AirplaneId"] = new SelectList(await _unitOfWork.Airplanes.GetAllAsync(), "Id", "Name", airplaneColor.AirplaneId);
            return View(airplaneColor);
        }

        // GET: AirplaneColors/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id,
                include: e => e.Include(s => s.Images)
                    .Include(s => s.AirplaneSizes)!
                    .ThenInclude(e => e.Size)!);

            if (airplaneColor == null)
            {
                return NotFound();
            }

            ViewData["ColorId"] =
                new SelectList(await _unitOfWork.Colors.GetAllAsync(), "Id", "Name", airplaneColor.ColorId);
            ViewData["AirplaneId"] = new SelectList(await _unitOfWork.Airplanes.GetAllAsync(), "Id", "Name", airplaneColor.AirplaneId);
            // ViewBag.SizeId = new SelectList(_context.Sizes, "Id", "Value");
            ViewBag.Sizes = await _unitOfWork.Sizes.GetAllAsync();

            return View(airplaneColor);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,FactoryPrice,SalePrice,SortOrder,Active,AirplaneId,ColorId")]
            AirplaneColor airplaneColor)
        {
            var airplaneColorFromDb = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id);
            if (id != airplaneColor.Id || airplaneColorFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                airplaneColor.Created = airplaneColorFromDb.Created;
                airplaneColor.Edited = DateTime.Now;
                _unitOfWork.AirplaneColors.Update(airplaneColor);
                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction(nameof(Edit));
            }

            ViewData["ColorId"] =
                new SelectList(await _unitOfWork.Colors.GetAllAsync(), "Id", "Name", airplaneColor.ColorId);
            ViewData["AirplaneId"] = new SelectList(await _unitOfWork.Airplanes.GetAllAsync(), "Id", "Name", airplaneColor.AirplaneId);
            ViewData["SizeId"] = new SelectList(await _unitOfWork.Sizes.GetAllAsync(), "id", "name");
            return View(airplaneColor);
        }

        public async Task<IActionResult> Delete(int id)
        {
            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id,
                include: e => e.Include(s => s.Images)
                    .Include(s => s.AirplaneSizes)!
                    .ThenInclude(e => e.Size));

            if (airplaneColor == null)
            {
                return NotFound();
            }

            ViewData["ColorId"] =
                new SelectList(new[] { await _unitOfWork.Colors.FirstOrDefaultAsync(e => e.Id == airplaneColor.ColorId) },
                    "Id", "Name", airplaneColor.ColorId);
            ViewData["AirplaneId"] =
                new SelectList(new[] { await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == airplaneColor.AirplaneId) },
                    "Id", "Name", airplaneColor.AirplaneId);
            // ViewBag.SizeId = new SelectList(_context.Sizes, "Id", "Value");
            ViewBag.Sizes = await _unitOfWork.Sizes.GetAllAsync();

            return View(airplaneColor);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneColor == null)
            {
                return NotFound();
            }
            
            var airplaneSizes = await _unitOfWork.AirplaneSizes.GetAllAsync(e => e.AirplaneColorId == airplaneColor.Id);
            
            if(airplaneSizes.Count == 0)
            {
                List<Image> airplaneImages = await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == airplaneColor.Id);
                if (airplaneImages != null && airplaneImages.Count > 0)
                {
                    var root = _webHostEnvironment.WebRootPath;
                    foreach (var image in airplaneImages)
                    {
                        string imageUrl = image.Path;
                        var imagePath = Path.Combine(root, imageUrl.TrimStart('\\'));
                        if (System.IO.File.Exists(imagePath))
                        {
                            System.IO.File.Delete(imagePath);
                        }
                    }
                }

                _unitOfWork.AirplaneColors.Remove(airplaneColor);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                TempData[SD.Error] = "Some airplanes is belong to this AirplaneColor. Can not delete it!";
            }
            
            return RedirectToAction("Edit", "Airplane", new {id = airplaneColor.AirplaneId });
        }
    }
}