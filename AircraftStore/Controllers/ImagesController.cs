using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Aircraft.DataAccess.Repository.IRepository;
using Aircraft.Models;
using Aircraft.Ultitity;

namespace Aircraft.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = SD.Role_Admin)]
    public class ImagesController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ImagesController(IWebHostEnvironment webHostEnvironment, IUnitOfWork unitOfWork)
        {
            _webHostEnvironment = webHostEnvironment;
            _unitOfWork = unitOfWork;
        }

        // GET: api/Images
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Image>>> GetImages()
        {
            return await _unitOfWork.Images.GetAllAsync(orderBy: e => e.SortOrder);
        }

        // GET: api/Images/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Image>> GetImage(int id)
        {
            var image = await _unitOfWork.Images.FirstOrDefaultAsync(e => e.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            return image;
        }

        // PUT: api/Images/5
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPut("sortOrder/{id}")]
        public async Task<ActionResult<IEnumerable<Image>>> PutImage(int id, int sortOrder)
        {
            var image = await _unitOfWork.Images.FirstOrDefaultAsync(e => e.Id == id);
            var sortOrderNew = sortOrder;

            if (image == null)
            {
                return NotFound();
            }

            var imageList = await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == image.AirplaneColorId,
                orderBy: s => s.SortOrder);
            if (sortOrderNew <= 0 || sortOrderNew > imageList.Count)
            {
                // return BadRequest();
                return Forbid();
            }

            imageList.RemoveAll(e => e.Id == image.Id);

            imageList.Insert(sortOrderNew - 1, image);
            for (var i = 0; i < imageList.Count; i++)
            {
                var img = imageList[i];
                img.SortOrder = i + 1;
            }

            _unitOfWork.Images.UpdateRange(imageList);
            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == image.AirplaneColorId,
                orderBy: s => s.SortOrder);
        }
        // [HttpPut("{id}")]
        // public async Task<IActionResult> PutImage(int id, Image image)
        // {
        //     if (id != image.Id)
        //     {
        //         return BadRequest();
        //     }
        //
        //     _context.Entry(image).State = EntityState.Modified;
        //
        //     try
        //     {
        //         await _context.SaveChangesAsync();
        //     }
        //     catch (DbUpdateConcurrencyException)
        //     {
        //         if (!ImageExists(id))
        //         {
        //             return NotFound();
        //         }
        //         else
        //         {
        //             throw;
        //         }
        //     }
        //
        //     return NoContent();
        // }

        // POST: api/Images
        // To protect from overposting attacks, see https://go.microsoft.com/fwlink/?linkid=2123754
        [HttpPost]
        public async Task<ActionResult<IEnumerable<Image>>> PostImage(int airplaneColorId, IFormFileCollection files)
        {
            AirplaneColor? airplaneColor =
                await _unitOfWork.AirplaneColors.FirstOrDefaultAsync(e => e.Id == airplaneColorId);
            if (airplaneColor == null)
            {
                return NotFound();
            }

            List<Image> images = await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == airplaneColorId,
                orderBy: s => s.SortOrder);

            int count = images.Count();
            // var imageUrls = new List<string>();
            string root = _webHostEnvironment.WebRootPath;

            foreach (var formFile in files)
            {
                string fileName = Guid.NewGuid().ToString();
                string fileExtension = Path.GetExtension(formFile.FileName);
                string filePath = Path.Combine(root, @"images\airplanes", fileName + fileExtension);
                await using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await formFile.CopyToAsync(fileStream);
                }

                var image = new Image
                {
                    AirplaneColorId = airplaneColorId,
                    Path = @"\images\airplanes\" + fileName + fileExtension,
                    SortOrder = ++count
                };
                await _unitOfWork.Images.AddAsync(image);
            }

            images = await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == airplaneColorId);

            for (var i = 0; i < images.Count; i++)
            {
                var img = images[i];
                img.SortOrder = i + 1;
            }

            _unitOfWork.Images.UpdateRange(images);

            await _unitOfWork.SaveChangesAsync();

            return await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == airplaneColorId,
                orderBy: e => e.SortOrder);

            // return CreatedAtAction("GetImage", new { id = image.Id }, image);
        }

        // DELETE: api/Images/5
        [HttpDelete("{id}")]
        public async Task<ActionResult<IEnumerable<Image>>> DeleteImage(int id)
        {
            var image = await _unitOfWork.Images.FirstOrDefaultAsync(e => e.Id == id);

            if (image == null)
            {
                return NotFound();
            }

            int airplaneColorId = image.AirplaneColorId;

            var root = _webHostEnvironment.WebRootPath;
            string imageUrl = image.Path;
            var imagePath = Path.Combine(root, imageUrl.TrimStart('\\'));
            if (System.IO.File.Exists(imagePath))
            {
                System.IO.File.Delete(imagePath);
            }

            _unitOfWork.Images.Remove(image);
            await _unitOfWork.SaveChangesAsync();

            var imageList = await _unitOfWork.Images.GetAllAsync(e => e.AirplaneColorId == airplaneColorId,
                orderBy: e => e.SortOrder);
            for (var i = 0; i < imageList.Count; i++)
            {
                var img = imageList[i];
                img.SortOrder = i + 1;
            }

            _unitOfWork.Images.UpdateRange(imageList);
            await _unitOfWork.SaveChangesAsync();

            return imageList.OrderBy(e => e.SortOrder).ToList();
        }
    }
}