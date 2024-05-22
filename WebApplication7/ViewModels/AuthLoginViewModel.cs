using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace WebApplication7.WebApi.ViewModels
{
    public class AuthLoginViewModel
    {
        [Required(ErrorMessage = "T.C. Kimlik Numarası gerekli.")]
        [RegularExpression("^[1-9]{1}[0-9]{10}$", ErrorMessage = "Geçerli bir T.C. Kimlik Numarası giriniz.")]
        [StringLength(11, MinimumLength = 11, ErrorMessage = "T.C. Kimlik Numarası 11 haneli olmalıdır.")]
        public string TC { get; set; }

        [Required(ErrorMessage = "Şifre gerekli.")]
        [StringLength(100, ErrorMessage = "Şifre en az {2} karakter uzunluğunda olmalıdır.", MinimumLength = 6)]
        [Display(Name = "Parola")]
        public string Parola { get; set; }
    }
}
