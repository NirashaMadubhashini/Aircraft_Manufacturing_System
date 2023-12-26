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
    [Authorize(Roles = SD.Role_Admin)]
    public class SizesApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public SizesApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/SizesApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Size>>> GetSizes()
        {
            return await _unitOfWork.Sizes.GetAllAsync(orderBy: e => e.Value);
        }

        [HttpGet("insertSize")]
        public async Task<ActionResult<IEnumerable<Size>>> GetSizes(int airplaneColorId)
        {
            AirplaneColor? airplaneColor = await _unitOfWork.AirplaneColors
                .FirstOrDefaultAsync(e => e.Id == airplaneColorId,
                    include: o => o.Include(e => e.AirplaneSizes)!);
            if (airplaneColor == null)
            {
                return BadRequest();
            }

            var oldSizeList = airplaneColor.AirplaneSizes?.Select(e => e.SizeId);

            var newSizeList = await _unitOfWork.Sizes.GetAllAsync(orderBy: e => e.Value);

            newSizeList.RemoveAll(e => oldSizeList != null && oldSizeList.Contains(e.Id));

            return newSizeList;
        }

        // GET: api/SizesApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Size>> GetSize(int id)
        {
            var size = await _unitOfWork.Sizes.FirstOrDefaultAsync(e => e.Id == id);

            if (size == null)
            {
                return NotFound();
            }

            return size;
        }

        // PUT: api/SizesApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutSize(int id, Size size)
        {
            if (id != size.Id)
            {
                return BadRequest();
            }

            // _context.Entry(size).State = EntityState.Modified;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!SizeExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/SizesApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<Size>> PostSize(Size size)
        {
            await _unitOfWork.Sizes.AddAsync(size);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction("GetSize", new { id = size.Id }, size);
        }

        // DELETE: api/SizesApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSize(int id)
        {
            var size = await _unitOfWork.Sizes.FirstOrDefaultAsync(e => e.Id == id);
            if (size == null)
            {
                return NotFound();
            }

            _unitOfWork.Sizes.Remove(size);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private bool SizeExists(int id)
        {
            return (_unitOfWork.Sizes?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}