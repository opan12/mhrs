using WebApplication7.Models.Enum;

namespace WebApplication7.Models.EntityBase
{
    public class Kisi
        
    {

        public int ID { get; set; }
        public string Ad { get; set; }
        public string Soyad { get; set; }
        public Rol Rol { get; set; }
    }
}
