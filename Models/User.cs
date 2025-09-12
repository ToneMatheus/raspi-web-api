namespace api_raspi_web.Models
{
    public class User
    {
        public int UserId { get; set; }
        public string Username { get; set; }
        //public string Email { get; set; }
        public string Passwd { get; set; }
        public DateTime CreatedAt { get; set; }
        //public DateTime UpdatedAt { get; set; }
    }
}
