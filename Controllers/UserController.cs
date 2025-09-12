using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.AspNetCore.Mvc;

namespace api_raspi_web.Controllers
{
    [ApiController]
    [Route("api")]
    public class UserController : ControllerBase
    {
        private readonly RaspidbContext _context;
        public UserController(RaspidbContext context)
        {
            _context = context;
        }
        [HttpGet("user/all")]
        public ActionResult<IEnumerable<User>> GetAllUsers()
        {
            var user = _context.User.ToList();

            if (!user.Any())
            {
                return NotFound("No balance found.");
            }

            return user;
        }
    }
}
