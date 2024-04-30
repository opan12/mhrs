namespace WebApplication7.Models.EntityBase
{
    public class Doktor:Kisi
    {
        public string UzmanlikAlani { get; set; }
        public string CalistigiHastane { get; set; }
         public virtual ICollection<Randevu> Randevular { get; set; }
         public virtual ICollection<TibbiRapor> TibbiRaporlar { get; set; }

    }
}
