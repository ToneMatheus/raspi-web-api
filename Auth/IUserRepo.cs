using api_raspi_web.Models;

namespace api_raspi_web.Auth
{
    public interface IUserRepo
    {
        Task<User?> FindByUsernameAsync(string username);
    }
}
