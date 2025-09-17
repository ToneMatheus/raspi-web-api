using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class CanItemUserController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public CanItemUserController(RaspidbContext context)
        {
            _context = context;
        }

        [HttpGet("canitemuser/all/{userId}")]
        public ActionResult<IEnumerable<CanItemUser>> GetAllItems(int userId)
        {
            var canItemUser = _context.CanItemUser.Where(i => i.UserId == userId).ToList();

            if (canItemUser == null || !canItemUser.Any())
                return NotFound();

            return Ok(canItemUser);
        }

        [HttpGet("canitemuser/{id:int}/{userId:int}")]
        public async Task<ActionResult<CanItem>> GetItemById(int id, int userId)
        {
            var canItemUser = await _context.CanItemUser
                .FirstOrDefaultAsync(c => c.CanItemUserId == id && c.UserId == userId);

            if (canItemUser is null)
                return NotFound();

            return Ok(canItemUser);
        }



        [HttpPost("canitemuser/create/{userId}")]
        public async Task<ActionResult<CanItemUser>> CreateItemUserEmpty(CanItemUser canItemUser, int userId)
        {
            if (canItemUser is null)
                return BadRequest("Invalid item data.");

            // Add the item
            _context.CanItemUser.Add(canItemUser);

            // Get the latest balance
            var latestBalance = await _context.CanBalanceUser
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CanBalanceUserId)
                .FirstOrDefaultAsync();

            decimal newTotal;
            if (latestBalance != null)
            {
                newTotal = latestBalance.Total - canItemUser.Price;
            }
            else
            {
                // If no balance exists yet, maybe treat initial balance as 0
                newTotal = -canItemUser.Price;
            }

            // Create and add the new balance
            var newBalance = new CanBalanceUser
            {
                Total = newTotal,
                UserId = userId
            };
            _context.CanBalanceUser.Add(newBalance);

            await _context.SaveChangesAsync();

            return CreatedAtAction(
                nameof(GetItemById),
                new { id = canItemUser.CanItemUserId, userid = canItemUser.CanItemUserId },
                canItemUser);
        }

        [HttpDelete("canitemuser/del/{id}/{userId}")]
        public async Task<IActionResult> DeleteItemById(int id, int userId)
        {
            var canItem = await _context.CanItemUser.FirstOrDefaultAsync(i => i.CanItemUserId == id && i.UserId == userId);

            if (canItem == null)
            {
                return NotFound();
            }

            // Remove the item
            _context.CanItemUser.Remove(canItem);

            // Remove the latest balance entry (corresponding to this item's addition)
            var latestBalance = await _context.CanBalanceUser
                .Where(b => b.UserId == userId)
                .OrderByDescending(b => b.CanBalanceUserId)
                .FirstOrDefaultAsync();

            if (latestBalance != null)
            {
                _context.CanBalanceUser.Remove(latestBalance);
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

    }
}
