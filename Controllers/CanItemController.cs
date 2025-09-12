using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class CanItemController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public CanItemController(RaspidbContext context)
        {
            _context = context;
        }

        [HttpGet("canitem/all")]
        public ActionResult<IEnumerable<CanItem>> GetAllItems()
        {
            var canItem = _context.CanItem.ToList();

            if (!canItem.Any())
            {
                return NotFound("No items found.");
            }

            return canItem;
        }

        [HttpGet("canitem/{id}")]
        public async Task<ActionResult<CanItem>> GetItemById(int id)
        {
            var canItem = await _context.CanItem.FindAsync(id);
            if (canItem is null) return NotFound();
            return canItem;
        }



        [HttpPost("canitem/create")]
        public async Task<ActionResult<CanItem>> CreateSaleEmpty(CanItem canItem)
        {
            if (canItem is null)
                return BadRequest("Invalid item data.");

            // Add the item
            _context.CanItem.Add(canItem);

            // Get the latest balance
            var latestBalance = await _context.CanBalance
                .OrderByDescending(b => b.CanBalanceId)
                .FirstOrDefaultAsync();

            decimal newTotal;
            if (latestBalance != null)
            {
                newTotal = latestBalance.Total - canItem.Price;
            }
            else
            {
                // If no balance exists yet, maybe treat initial balance as 0
                newTotal = -canItem.Price;
            }

            // Create and add the new balance
            var newBalance = new CanBalance
            {
                Total = newTotal
            };
            _context.CanBalance.Add(newBalance);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetItemById),
                new { id = canItem.CanItemId },
                canItem);
        }

        [HttpDelete("canitem/del/{id}")]
        public async Task<IActionResult> DeleteItemById(int id)
        {
            var canItem = await _context.CanItem.FindAsync(id);
            if (canItem == null)
            {
                return NotFound();
            }

            // Remove the item
            _context.CanItem.Remove(canItem);

            // Remove the latest balance entry (corresponding to this item's addition)
            var latestBalance = await _context.CanBalance
                .OrderByDescending(b => b.CanBalanceId)
                .FirstOrDefaultAsync();

            if (latestBalance != null)
            {
                _context.CanBalance.Remove(latestBalance);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
