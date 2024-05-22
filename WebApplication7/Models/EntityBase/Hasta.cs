namespace WebApplication7.Models.EntityBase
{
    public class Hasta:Kisi
    {
        public int HastaID { get; set; }

        public DateTime? DogumTarihi { get; set; }
        public string Cinsiyet { get; set; }
        public string TelefonNumarasi { get; set; }
        public string Adres { get; set; }
        public string TC { get; set; }
        public string Parola { get; set; }
        public int Rol { get; set; }  // Rol alanı eklendi
       // public virtual ICollection<Randevu> Randevular { get; set; }
     //   public virtual ICollection<TibbiRapor> TibbiRaporlar { get; set; }
    }
}