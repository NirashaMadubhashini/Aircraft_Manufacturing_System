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
    public class AirplaneColorApiController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;

        public AirplaneColorApiController(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        // GET: api/AirplaneColorApi
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AirplaneColor>>> GetAirplaneColor()
        {

            return await _unitOfWork.AirplaneColors.GetAllAsync();
        }

        // GET: api/AirplaneColorApi/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AirplaneColor>> GetAirplaneColor(int id)
        {

            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id);

            if (airplaneColor == null)
            {
                return NotFound();
            }

            return airplaneColor;
        }

        // PUT: api/AirplaneColorApi/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("{id}")]
        public async Task<IActionResult> PutAirplaneColor(int id, AirplaneColor airplaneColor)
        {
            if (id != airplaneColor.Id)
            {
                return BadRequest();
            }

            // _context.Entry(airplaneColor).State = EntityState.Modified;

            try
            {
                await _unitOfWork.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AirplaneColorExists(id))
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

        // POST: api/AirplaneColorApi
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<AirplaneColor>> PostAirplaneColor(AirplaneColor airplaneColor)
        {

            await _unitOfWork.AirplaneColors.AddAsync(airplaneColor);
            await _unitOfWork.SaveChangesAsync();

            return CreatedAtAction("GetAirplaneColor", new { id = airplaneColor.Id }, airplaneColor);
        }

        // DELETE: api/AirplaneColorApi/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAirplaneColor(int id)
        {

            var airplaneColor = await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == id);
            if (airplaneColor == null)
            {
                return NotFound();
            }

            _unitOfWork.AirplaneColors.Remove(airplaneColor);
            await _unitOfWork.SaveChangesAsync();

            return NoContent();
        }

        private bool AirplaneColorExists(int id)
        {
            return (_unitOfWork.AirplaneColors?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}