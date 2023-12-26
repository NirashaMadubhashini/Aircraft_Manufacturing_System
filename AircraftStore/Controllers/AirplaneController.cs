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
    public class AirplaneController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AirplaneController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IActionResult> Index()
        {
            var airplanes = _unitOfWork.Airplanes.GetAllAsync(include: e => e.Include(s => s.Brand)!);
            return View(await airplanes);
        }

        public async Task<IActionResult> Details(int id)
        {
            var airplane = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == id,
                include: e =>
                    e.Include(s => s.Brand)
                        .Include(s => s.Category)
                        .Include(s => s.AirplaneColors)!
                        .ThenInclude(s => s.Color)
                        .Include(s => s.AirplaneColors)!
                        .ThenInclude(s => s.Images)!
            );

            if (airplane == null)
            {
                return NotFound();
            }

            return View(airplane);
        }

        public async Task<IActionResult> Create()
        {
            ViewBag.BrandId = new SelectList(await _unitOfWork.Brands.GetAllAsync(), "Id", "Name");
            ViewBag.CategoryId = new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name");
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(
            [Bind("Id,Name,ProductCode,Priority,Active,Features,Description,Note,BrandId,CategoryId")]
            Airplane airplane)
        {
            if (ModelState.IsValid)
            {
                airplane.Created = DateTime.Now;
                airplane.Edited = DateTime.Now;
                await _unitOfWork.Airplanes.AddAsync(airplane);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Edit), new { id = airplane.Id });
            }

            // ViewData["BrandId"] = new SelectList(await _unitOfWork.Brands.GetAllAsync(), "Id", "Unit", airplane.BrandId);
            // return View(airplane);
            return await Create();
        }

        public async Task<IActionResult> Edit(int id)
        {
            var airplane = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == id,
                include: o => o.Include(e => e.AirplaneColors)!
                    .ThenInclude(e => e.Color)
                    .Include(e => e.AirplaneColors)!
                    .ThenInclude(e => e.Images)!);

            if (airplane == null)
            {
                return NotFound();
            }

            ViewBag.BrandId = new SelectList(await _unitOfWork.Brands.GetAllAsync(), "Id", "Name", airplane.BrandId);
            ViewBag.CategoryId =
                new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Name", airplane.CategoryId);

            return View(airplane);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Name,ProductCode,Active,Features,Description,Note,BrandId,CategoryId")]
            Airplane airplane)
        {
            var airplaneFromDb = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneFromDb == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                airplane.Created = airplaneFromDb.Created;
                airplane.Edited = DateTime.Now;
                _unitOfWork.Airplanes.Update(airplane);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewBag.BrandId = new SelectList(await _unitOfWork.Brands.GetAllAsync(), "Id", "Unit", airplane.BrandId);
            ViewBag.CategoryId =
                new SelectList(await _unitOfWork.Categories.GetAllAsync(), "Id", "Unit", airplane.CategoryId);
            return View(airplane);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            var airplane = await _unitOfWork.Airplanes
                .FirstOrDefaultAsync(m => m.Id == id,
                    include: o => o.Include(s => s.Brand)
                        .Include(s => s.Category)!);

            if (airplane == null)
            {
                return NotFound();
            }

            return View(airplane);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplane = await _unitOfWork.Airplanes.FirstOrDefaultAsync(e => e.Id == id);
            if (airplane == null)
            {
                return NotFound();
            }

            var airplaneColors = await _unitOfWork.AirplaneColors.GetAllAsync(e => e.AirplaneId == airplane.Id);
            if (airplaneColors.Count == 0)
            {
                _unitOfWork.Airplanes.Remove(airplane);
                await _unitOfWork.SaveChangesAsync();
            }
            else
            {
                TempData[SD.Error] = "Some AirplaneColorModels is belong to this Airplane Model. Can not delete it!";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}