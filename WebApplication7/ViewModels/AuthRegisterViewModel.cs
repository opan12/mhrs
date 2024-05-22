using System.ComponentModel.DataAnnotations;

namespace WebApplication7.WebApi.ViewModels
{
    public class AuthRegisterViewModel
    {
        [Required]
        public string Ad { get; set; }
        [Required]
      
        public string soyad { get; set; }
        [Required]
       
        public string Parola { get; set; }
        [Required]
        public string TC { get; set; }
     
       
    }
}