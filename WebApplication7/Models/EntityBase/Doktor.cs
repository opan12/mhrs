using System.ComponentModel.DataAnnotations;

namespace WebApplication7.Models.EntityBase
{
    public class Doktor:Kisi
    {
        public int DoktorID { get; set; }

        [Required(ErrorMessage = "Uzmanlık Alanı gereklidir.")]
        public string UzmanlikAlani { get; set; }

        [Required(ErrorMessage = "Çalıştığı Hastane gereklidir.")]
        public string CalistigiHastane { get; set; }

        [Required(ErrorMessage = "TC gereklidir.")]
        public string TC { get; set; }

        [Required(ErrorMessage = "Parola gereklidir.")]
        public string Parola { get; set; }

        public int Rol { get; set; }

      //  public virtual ICollection<Randevu> Randevular { get; set; }

       // public virtual ICollection<TibbiRapor> TibbiRaporlar { get; set; }
    }
}