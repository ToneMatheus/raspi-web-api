using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class CanBalanceUserController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public CanBalanceUserController(RaspidbContext context)
        {
            _context = context;
        }
        public class ResetUserCanStateRequest
        {
            public int UserId { get; set; }
            public decimal NewBalance { get; set; }
        }

        [HttpPost("canbalanceuser/reset")]
        public async Task<IActionResult> ResetUserCanState([FromBody] ResetUserCanStateRequest request)
        {
            if (request is null || request.UserId <= 0)
                return BadRequest("Invalid request.");

            var userExists = await _context.User.AnyAsync(u => u.UserId == request.UserId);
            if (!userExists)
                return BadRequest($"User {request.UserId} does not exist.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Pick the user's anchor balance (oldest by PK). 
                // If you have IsDefault, use that instead:
                // var anchor = await _context.CanBalanceUser
                //     .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.IsDefault);

                var anchor = await _context.CanBalanceUser
                    .Where(b => b.UserId == request.UserId)
                    .OrderBy(b => b.CanBalanceUserId)
                    .FirstOrDefaultAsync();

                // Create an anchor if none exists
                if (anchor is null)
                {
                    anchor = new CanBalanceUser
                    {
                        UserId = request.UserId,
                        Total = request.NewBalance
                        // IsDefault = true
                    };
                    _context.CanBalanceUser.Add(anchor);
                    await _context.SaveChangesAsync(); // ensure anchor has an ID for FKs
                }

                // Collect ids for FK-safe deletes
                var itemIds = await _context.CanItemUser
                    .Where(i => i.UserId == request.UserId)
                    .Select(i => i.CanItemUserId)
                    .ToListAsync();

                var balanceIdsToDelete = await _context.CanBalanceUser
                    .Where(b => b.UserId == request.UserId && b.CanBalanceUserId != anchor.CanBalanceUserId)
                    .Select(b => b.CanBalanceUserId)
                    .ToListAsync();

                // 1) Delete transactions referencing those items or balances (but not the anchor)
                await _context.CanTransactionUser
                    .Where(t =>
                        (t.CanItemUserId != null && itemIds.Contains(t.CanItemUserId)) ||
                        (t.CanBalanceUserId != null && balanceIdsToDelete.Contains(t.CanBalanceUserId)))
                    .ExecuteDeleteAsync();

                // 2) Delete all user's items
                await _context.CanItemUser
                    .Where(i => i.UserId == request.UserId)
                    .ExecuteDeleteAsync();

                // 3) Delete all user's balances except the anchor
                if (balanceIdsToDelete.Count > 0)
                {
                    await _context.CanBalanceUser
                        .Where(b => balanceIdsToDelete.Contains(b.CanBalanceUserId))
                        .ExecuteDeleteAsync();
                }

                // 4) Update anchor balance
                anchor.Total = request.NewBalance;
                _context.CanBalanceUser.Update(anchor);

                // Optional: log a reset transaction tied to the anchor
                // _context.CanTransactionUser.Add(new CanTransactionUser {
                //     CanBalanceUserId = anchor.CanBalanceUserId,
                //     TransactionDate = DateTime.UtcNow,
                //     Amount = request.NewBalance, // if you record it
                //     Note = "Reset"
                // });

                await _context.SaveChangesAsync();
                await tx.CommitAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                await tx.RollbackAsync();
                return Problem($"Reset failed: {ex.Message}");
            }
        }



        [HttpGet("canbalanceuser/{userId}")]
        public async Task<ActionResult<IEnumerable<CanBalanceUser>>> GetBalancesUser(int userId)
        {
            var canBalanceUser = await _context.CanBalanceUser.Where(b => b.UserId == userId).ToListAsync();

            if (canBalanceUser == null || !canBalanceUser.Any())
                return NotFound();

            return Ok(canBalanceUser);
        }
    }
}
