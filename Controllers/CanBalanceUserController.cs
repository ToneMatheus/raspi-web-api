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
