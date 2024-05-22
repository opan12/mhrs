namespace WebApplication7.Models.EntityBase
{
    public class Notification
    {
        public int NotificationID { get; set; }
        public DateTime Timestamp { get; set; }
        public string Message { get; set; }
        public int HastaID { get; set; }
    }
}