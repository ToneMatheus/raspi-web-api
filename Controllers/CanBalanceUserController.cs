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

            // Keep ONLY the balance with id=1 (must belong to this user)
            var anchor = await _context.CanBalanceUser
                .FirstOrDefaultAsync(b => b.UserId == request.UserId && b.CanBalanceUserId == 1);

            if (anchor is null)
                return BadRequest($"Balance with id=1 does not exist for user {request.UserId}.");

            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                // Collect ids for FK-safe deletes
                var itemIds = await _context.CanItemUser
                    .Where(i => i.UserId == request.UserId)
                    .Select(i => i.CanItemUserId)
                    .ToListAsync();

                var balanceIdsToDelete = await _context.CanBalanceUser
                    .Where(b => b.UserId == request.UserId && b.CanBalanceUserId != 1)
                    .Select(b => b.CanBalanceUserId)
                    .ToListAsync();

                // 1) Delete transactions referencing those items or balances
                await _context.CanTransactionUser
                    .Where(t =>
                        (t.CanItemUserId != null && itemIds.Contains(t.CanItemUserId)) ||
                        (t.CanBalanceUserId != null && balanceIdsToDelete.Contains(t.CanBalanceUserId)))
                    .ExecuteDeleteAsync();

                // 2) Delete all user's items
                await _context.CanItemUser
                    .Where(i => i.UserId == request.UserId)
                    .ExecuteDeleteAsync();

                // 3) Delete all user's balances except id=1
                await _context.CanBalanceUser
                    .Where(b => b.UserId == request.UserId && b.CanBalanceUserId != 1)
                    .ExecuteDeleteAsync();

                // 4) Update anchor balance
                anchor.Total = request.NewBalance;
                _context.CanBalanceUser.Update(anchor);

                // Optional: log a "reset" transaction
                // _context.CanTransactionUser.Add(new CanTransactionUser {
                //     CanBalanceUserId = anchor.CanBalanceUserId,
                //     TransactionDate = DateTime.UtcNow
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
