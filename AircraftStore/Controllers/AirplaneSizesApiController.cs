using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Data;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Ultitity;

namespace Aircraft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    // [Authorize(Roles = SD.Role_Admin)]
    public class AirplaneSizesApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AirplaneSizesApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/AirplaneSizesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirplaneSize>>> GetAirplaneSize()
        {
            return await _unitOfWork.AirplaneSizes.GetAllAsync();
        }

        // GET: api/AirplaneSizesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AirplaneSize>> GetAirplaneSize(int id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id);

            if (airplaneSize == null)
            {
                return NotFound();
            }

            return airplaneSize;
        }

        // PUT: api/AirplaneSizesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<ActionResult<IEnumerable<AirplaneSize>>> PutAirplaneSize(int id, AirplaneSize airplaneSize)
        {
            if (id != airplaneSize.Id)
            {
                return BadRequest();
            }

            AirplaneSize? airplaneSizeFromDb = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id);

            if (airplaneSizeFromDb == null || (airplaneSize.Quantity is <= 0 or > 99))
            {
                return BadRequest();
            }

            airplaneSizeFromDb.Quantity = airplaneSize.Quantity;

            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.AirplaneSizes.GetAllAsync(e => e.AirplaneColorId == airplaneSizeFromDb.AirplaneColorId,
                include: o => o.Include(e => e.Size)!,
                orderBy: e => e.Size!.Value);
        }

        // POST: api/AirplaneSizesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IEnumerable<AirplaneSize>>> PostAirplaneSize(AirplaneSize airplaneSize)
        {
            AirplaneColor? airplaneColor = await _unitOfWork.AirplaneColors
                .FirstOrDefaultAsync(e => e.Id == airplaneSize.AirplaneColorId);
            Size? size = await _unitOfWork.Sizes
                .FirstOrDefaultAsync(e => e.Id == airplaneSize.SizeId);

            AirplaneSize? duplicateAirplaneSize = await _unitOfWork.AirplaneSizes
                .FirstOrDefaultAsync(e => e.SizeId == airplaneSize.Id && e.AirplaneColorId == airplaneSize.AirplaneColorId);

            if (airplaneColor == null || size == null || duplicateAirplaneSize != null || airplaneSize.Quantity <= 0 ||
                airplaneSize.Quantity > 99)
            {
                return BadRequest();
            }

            await _unitOfWork.AirplaneSizes.AddAsync(airplaneSize);
            await _unitOfWork.SaveChangesAsync();

            // return CreatedAtAction("GetAirplaneSize", new { id = airplaneSize.Id }, airplaneSize);
            return await _unitOfWork.AirplaneSizes.GetAllAsync(e => e.AirplaneColorId == airplaneSize.AirplaneColorId,
                include: o => o.Include(e => e.Size),
                orderBy: e => e.Size!.Value);
        }

        // DELETE: api/AirplaneSizesApi/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<AirplaneSize>>> DeleteAirplaneSize(int id)
        {
            var airplaneSize = await _unitOfWork.AirplaneSizes.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneSize == null)
            {
                return NotFound();
            }

            int airplaneColorId = airplaneSize.AirplaneColorId;

            _unitOfWork.AirplaneSizes.Remove(airplaneSize);
            await _unitOfWork.SaveChangesAsync();

            // return NoContent();
            return await _unitOfWork.AirplaneSizes.GetAllAsync(e => e.AirplaneColorId == airplaneColorId,
                include: o => o.Include(e => e.Size),
                orderBy: e => e.Size!.Value);
        }

        private bool AirplaneSizeExists(int id)
        {
            return (_unitOfWork.AirplaneSizes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}