using WebApplication7.Models.Enum;

namespace WebApplication7.Models.IEntityBase
{
    public interface IEntityBase
    {
        int ID { get; set; }
        string Ad { get; set; }
        string Soyad { get; set; }
        Rol Rol { get; set; }
    }
}