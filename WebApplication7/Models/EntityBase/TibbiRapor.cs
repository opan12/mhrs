namespace WebApplication7.Models.EntityBase
{
    public class TibbiRapor
    {
        public int RaporID { get; set; }
        public DateTime RaporTarihi { get; set; }
        public string RaporIcerigi { get; set; }
        public string RaporURL { get; set; }
        public int HastaID { get; set; }
        public int DoktorID { get; set; }
      
        public IFormFile? File { get; set; }

    }
}