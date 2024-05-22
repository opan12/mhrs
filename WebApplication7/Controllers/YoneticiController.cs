using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;

namespace WebApplication7.Controllers
{
    public class YoneticiController : Controller
    {
        private readonly string _connectionString;

        public YoneticiController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        [HttpGet]
        public IActionResult YoneticiAnaSayfa()
        {
            // Yönetici ana sayfası, burada gerekirse yönetici için bir dashboard oluşturabilirsiniz.
            return View();
        }

        [HttpGet]
        public IActionResult HastaEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult HastaEkle(Hasta hasta)
        {
            try
            {
                AddHasta(hasta);
                return RedirectToAction("YoneticiAnaSayfa");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hasta ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpGet]
        public IActionResult DoktorEkle()
        {
            return View();
        }

        [HttpPost]
        public IActionResult DoktorEkle(Doktor doktor)
        {
            try
            {
                AddDoktor(doktor);
                return RedirectToAction("YoneticiAnaSayfa");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Doktor ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

  

 
        [HttpGet]
        public IActionResult TibbiRaporEkleme()
        {
            return View();
        }

        [HttpPost]
        public IActionResult TibbiRaporEkleme(TibbiRapor rapor)
        {
            try
            {
                AddTibbiRapor(rapor);
                return RedirectToAction("YoneticiAnaSayfa");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Tıbbi rapor ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }
        private void AddHasta(Hasta hasta)
        {
            // Hasta ekleme işlemi için veritabanına sorgu gönder
            // Veritabanı işlemleri için kullanılan kodu buraya ekleyin
        }

        private void AddDoktor(Doktor doktor)
        {
            // Doktor ekleme işlemi için veritabanına sorgu gönder
            // Veritabanı işlemleri için kullanılan kodu buraya ekleyin
        }

        private void AddTibbiRapor(TibbiRapor rapor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var command = new SqlCommand("INSERT INTO TibbiRapor (RaporTarihi, RaporIcerigi, RaporURL, HastaID, DoktorID) VALUES (@RaporTarihi, @RaporIcerigi, @RaporURL, @HastaID, @DoktorID)", connection);
                command.Parameters.AddWithValue("@RaporTarihi", rapor.RaporTarihi);
                command.Parameters.AddWithValue("@RaporIcerigi", rapor.RaporIcerigi);
                command.Parameters.AddWithValue("@RaporURL", rapor.RaporURL);
                command.Parameters.AddWithValue("@HastaID", rapor.HastaID);
                command.Parameters.AddWithValue("@DoktorID", rapor.DoktorID);

                connection.Open();
                command.ExecuteNonQuery();
            }
        }
    }
}
