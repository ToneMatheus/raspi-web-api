namespace api_raspi_web.Auth
{
    record LoginDto(string Email, string Password);

    // simple example repo contract
    public interface IUserRepo
    {
        Task<User?> FindByEmailAsync(string email);
    }
    public record User(Guid Id, string Email, string PasswordHash);
}
