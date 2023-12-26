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
    public class AirplaneSizesController : Controller
    {
        private readonly IUnitOfWork _unitOfWork;

        public AirplaneSizesController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: AirplaneSizes
        public async Task<IActionResult> Index()
        {
            var airplaneSizes = _unitOfWork.AirplaneSizes.GetAllAsync(include: e => e.Include(s => s.AirplaneColor)
                .Include(s => s.Size)!);
            return View(await airplaneSizes);
        }

        // GET: AirplaneSizes/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id,
                include: e => e.Include(s => s.AirplaneColor)
                    .Include(s => s.Size)!);
            if (airplaneSize == null)
            {
                return NotFound();
            }

            return View(airplaneSize);
        }

        // GET: AirplaneSizes/Create
        public async Task<IActionResult> Create(int airplaneColorId)
        {
            ViewData["AirplaneColorid"] =
                new SelectList(await _unitOfWork.AirplaneColors.GetAllAsync(), "Id", "Id", airplaneColorId);
            ViewData["SizeId"] = new SelectList(await _unitOfWork.Sizes.GetAllAsync(), "Id", "Unit");
            return View();
        }

        // POST: AirplaneSizes/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Quantity,Reorder,AirplaneColorid,SizeId")] AirplaneSize airplaneSize)
        {
            if (ModelState.IsValid)
            {
                await _unitOfWork.AirplaneSizes.AddAsync(airplaneSize);
                await _unitOfWork.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            ViewData["AirplaneColorid"] = new SelectList(await _unitOfWork.AirplaneColors.GetAllAsync(), "Id", "Id",
                airplaneSize.AirplaneColorId);
            ViewData["SizeId"] = new SelectList(await _unitOfWork.Sizes.GetAllAsync(), "Id", "Unit", airplaneSize.SizeId);
            return View(airplaneSize);
        }

        // GET: AirplaneSizes/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneSize == null)
            {
                return NotFound();
            }

            ViewData["AirplaneColorid"] = new SelectList(await _unitOfWork.AirplaneColors.GetAllAsync(), "Id", "Id",
                airplaneSize.AirplaneColorId);
            ViewData["SizeId"] = new SelectList(await _unitOfWork.Sizes.GetAllAsync(), "Id", "Unit", airplaneSize.SizeId);
            return View(airplaneSize);
        }

        // POST: AirplaneSizes/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id,
            [Bind("Id,Quantity,Reorder,AirplaneColorid,SizeId")] AirplaneSize airplaneSize)
        {
            if (id != airplaneSize.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                _unitOfWork.AirplaneSizes.Update(airplaneSize);
                await _unitOfWork.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            ViewData["AirplaneColorid"] = new SelectList(await _unitOfWork.AirplaneColors.GetAllAsync(), "Id", "Id",
                airplaneSize.AirplaneColorId);
            ViewData["SizeId"] = new SelectList(await _unitOfWork.Sizes.GetAllAsync(), "Id", "Unit", airplaneSize.SizeId);
            return View(airplaneSize);
        }

        // GET: AirplaneSizes/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id,
                include: e => e.Include(s => s.AirplaneColor)
                    .Include(s => s.Size)!);
            if (airplaneSize == null)
            {
                return NotFound();
            }

            return View(airplaneSize);
        }

        // POST: AirplaneSizes/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneSize != null)
            {
                _unitOfWork.AirplaneSizes.Remove(airplaneSize);
            }

            await _unitOfWork.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}