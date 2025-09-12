using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class BalanceController : ControllerBase
    {

        private readonly RaspidbContext _context;
        public BalanceController(RaspidbContext context) 
        {
            _context = context;
        }

        [HttpGet("balance/all")]
        public ActionResult<IEnumerable<Balance>> GetAllBalances()
        {
            var balance = _context.Balance.ToList();

            if (!balance.Any())
            {
                return NotFound("No balance found.");
            }

            return balance;
        }
    }
}
