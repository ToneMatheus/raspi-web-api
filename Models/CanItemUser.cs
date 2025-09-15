namespace api_raspi_web.Models
{
    public class CanItemUser
    {
        public int CanItemUserId { get; set; }
        public decimal Price { get; set; }
        public DateTime Date { get; set; }
        public string Description { get; set; }
        public int UserId { get; set; }
        public User User { get; set; }
    }
}
