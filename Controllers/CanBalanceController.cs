using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class CanBalanceController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public CanBalanceController(RaspidbContext context)
        {
            _context = context;
        }

        [HttpGet("canbalance/all")]
        public ActionResult<IEnumerable<CanBalance>> GetAllBalances()
        {
            var canBalance = _context.CanBalance.ToList();

            if (!canBalance.Any())
            {
                return NotFound("No balance found.");
            }

            return canBalance;
        }
    }
}
