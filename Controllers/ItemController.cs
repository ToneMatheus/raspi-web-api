using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class ItemController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public ItemController(RaspidbContext context)
        {
            _context = context;
        }

        [HttpGet("item/all")]
        public ActionResult<IEnumerable<Item>> GetAllItems()
        {
            var item = _context.Item.ToList();

            if (!item.Any())
            {
                return NotFound("No items found.");
            }

            return item;
        }

        [HttpGet("item/{id}")]
        public async Task<ActionResult<Item>> GetItemById(int id)
        {
            var item = await _context.Item.FindAsync(id);
            if (item is null) return NotFound();
            return item;
        }



        [HttpPost("item/create")]
        public async Task<ActionResult<Item>> CreateSaleEmpty(Item item)
        {
            if (item is null)
                return BadRequest("Invalid item data.");

            // Add the item
            _context.Item.Add(item);

            // Get the latest balance
            var latestBalance = await _context.Balance
                .OrderByDescending(b => b.BalanceId)
                .FirstOrDefaultAsync();

            decimal newTotal;
            if (latestBalance != null)
            {
                newTotal = latestBalance.Total - item.Price;
            }
            else
            {
                // If no balance exists yet, maybe treat initial balance as 0
                newTotal = -item.Price;
            }

            // Create and add the new balance
            var newBalance = new Balance
            {
                Total = newTotal
            };
            _context.Balance.Add(newBalance);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetItemById),
                new { id = item.ItemId },
                item);
        }

        [HttpDelete("item/del/{id}")]
        public async Task<IActionResult> DeleteItemById(int id)
        {
            var item = await _context.Item.FindAsync(id);
            if (item == null)
            {
                return NotFound();
            }

            // Remove the item
            _context.Item.Remove(item);

            // Remove the latest balance entry (corresponding to this item's addition)
            var latestBalance = await _context.Balance
                .OrderByDescending(b => b.BalanceId)
                .FirstOrDefaultAsync();

            if (latestBalance != null)
            {
                _context.Balance.Remove(latestBalance);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
