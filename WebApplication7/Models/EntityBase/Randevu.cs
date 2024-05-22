namespace WebApplication7.Models.EntityBase
{
    public class Randevu
    {
        public int RandevuID { get; set; }
        public DateTime RandevuTarihi { get; set; }
        public string RandevuSaati { get; set; }
        public int HastaID { get; set; }
        public int DoktorID { get; set; }
   

    }
}
