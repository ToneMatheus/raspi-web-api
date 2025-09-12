using api_raspi_web.Auth;
using api_raspi_web.Contexts;
using api_raspi_web.Models;
using Microsoft.EntityFrameworkCore;

namespace api_raspi_web.Auth
{

    public sealed class UserRepo : IUserRepo
    {
        private readonly RaspidbContext _db;
        public UserRepo(RaspidbContext db) => _db = db;

        public Task<User?> FindByUsernameAsync(string username) =>
            _db.User.AsNoTracking()
                     .FirstOrDefaultAsync(u => u.Username == username.Trim().ToLower());
    }
}
