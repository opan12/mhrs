using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using Microsoft.Data.SqlClient;
using WebApplication7.Models.EntityBase;

namespace WebApplication7.Controllers
{
    public class UserController : Controller
    {
        private readonly string _connectionString;

        public UserController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

      
   
     

        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Login(string TC, string parola)
        {
            try
            {
                using (var connection = new SqlConnection(_connectionString))
                {
                    var query = @"
                SELECT 'Hasta' AS UserType, HastaID AS UserID, Rol FROM Hasta WHERE TC = @TC AND Parola = @Parola
                UNION ALL
                SELECT 'Doktor', DoktorID, Rol FROM Doktor WHERE TC = @TC AND Parola = @Parola
                UNION ALL
                SELECT 'Yonetici', YoneticiID, Rol FROM Yonetici WHERE TC = @TC AND Parola = @Parola";

                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@TC", TC);
                        command.Parameters.AddWithValue("@Parola", parola);

                        connection.Open();
                        using (var reader = command.ExecuteReader())
                        {
                            if (reader.Read())
                            {
                                var userType = reader["UserType"].ToString();
                                var userId = Convert.ToInt32(reader["UserID"]);
                                var rol = Convert.ToInt32(reader["Rol"]);
                              
                                // Rol kontrolüne göre yönlendirme
                                if (rol == 1)
                                {
                                    TempData["DoktorID"] = userId;
                                    return RedirectToAction("Index", "Doktor");
                                }
                                else if (rol == 2)
                                {
                                    TempData["HastaID"] = userId;
                                    return RedirectToAction("Index", "Hasta");
                                }
                                else if (rol == 3)
                                {
                                    TempData["YoneticiID"] = userId;
                                    return RedirectToAction("YoneticiAnaSayfa", "Yonetici");
                                }
                                else
                                    return RedirectToAction("Index", "Home");
                            }
                            else
                            {
                                // Hatalı giriş, hata mesajı göster
                                ViewBag.ErrorMessage = "TC kimlik veya şifre yanlış.";
                                return View();
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Giriş işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }
    }
}
