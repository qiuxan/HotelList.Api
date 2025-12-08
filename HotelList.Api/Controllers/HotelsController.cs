using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using HotelList.Api.Data;

namespace HotelList.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HotelsController : ControllerBase
    {
        private static readonly List<Hotel> Hotels = new()
        {
            new Hotel { Id = 1, Name = "Seaside Resort", Address = "1 Ocean View", Rating = 4.5 },
            new Hotel { Id = 2, Name = "City Center Inn", Address = "25 Downtown Ave", Rating = 4.0 }
        };

        private static readonly object SyncRoot = new();

        [HttpGet]
        public ActionResult<IEnumerable<Hotel>> GetAll()
        {
            return Ok(Hotels);
        }

        [HttpGet("{id:int}")]
        public ActionResult<Hotel> GetById(int id)
        {
            var hotel = Hotels.FirstOrDefault(h => h.Id == id);
            if (hotel == null)
            {
                return NotFound();
            }

            return Ok(hotel);
        }

        [HttpPost]
        public ActionResult<Hotel> Create([FromBody] Hotel hotel)
        {
            if (hotel == null)
            {
                return BadRequest();
            }

            lock (SyncRoot)
            {
                var nextId = Hotels.Count == 0 ? 1 : Hotels.Max(h => h.Id) + 1;
                hotel.Id = nextId;
                Hotels.Add(hotel);
            }

            return CreatedAtAction(nameof(GetById), new { id = hotel.Id }, hotel);
        }

        [HttpPut("{id:int}")]
        public IActionResult Update(int id, [FromBody] Hotel updatedHotel)
        {
            if (updatedHotel == null)
            {
                return BadRequest();
            }

            lock (SyncRoot)
            {
                var existingHotel = Hotels.FirstOrDefault(h => h.Id == id);
                if (existingHotel == null)
                {
                    return NotFound();
                }

                existingHotel.Name = updatedHotel.Name;
                existingHotel.Address = updatedHotel.Address;
                existingHotel.Rating = updatedHotel.Rating;
            }

            return NoContent();
        }

        [HttpDelete("{id:int}")]
        public IActionResult Delete(int id)
        {
            lock (SyncRoot)
            {
                var hotel = Hotels.FirstOrDefault(h => h.Id == id);
                if (hotel == null)
                {
                    return NotFound();
                }

                Hotels.Remove(hotel);
            }

            return NoContent();
        }
    }
}
