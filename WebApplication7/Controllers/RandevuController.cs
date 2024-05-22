using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WebApplication7.Models.EntityBase;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication7.Controllers
{
    public class RandevuController : Controller
    {
        private readonly string _connectionString;

        public RandevuController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }
        [HttpGet]
        public IActionResult Index()
        {
            List<Randevu> randevular = GetRandevular();
            return View(randevular);
        }
      
        [HttpGet]
        public IActionResult Edit(int id)
        {
            var randevu = GetRandevuById(id);

            if (randevu == null)
            {
                return NotFound();
            }

            var doktorlar = GetDoktorlar();
            if (doktorlar == null || !doktorlar.Any())
            {
                ViewBag.Doktorlar = new SelectList(new List<Doktor>());
            }
            else
            {
                ViewBag.Doktorlar = new SelectList(doktorlar, "DoktorID", "Ad", randevu.DoktorID);
            }

            // Randevu için doktor adını ViewBag'e ekle
            ViewBag.DoktorAdi = GetDoktorAdiById(randevu.DoktorID);

            return View(randevu);
        }


        [HttpPost]
        public IActionResult Edit(Randevu randevu)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UpdateRandevu(randevu);
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Randevu güncelleme işlemi sırasında bir hata oluştu: " + ex.Message;
            }

            return View(randevu);
        }

        [HttpGet]
        public IActionResult Create(int? hastaId)
        {
            if (hastaId.HasValue)
            {
                ViewBag.HastaID = hastaId.Value;
            }
            ViewBag.UzmanlikAlanlari = GetUzmanlikAlanlari();
            return View();
        }

        [HttpPost]
        public IActionResult Create(Randevu randevu)
        {
            if (ModelState.IsValid)
            {
                AddRandevu(randevu);
                return RedirectToAction("Index", "Hasta");
            }
            ViewBag.UzmanlikAlanlari = GetUzmanlikAlanlari();
            return View(randevu);
        }

        [HttpGet]
        public JsonResult GetHastaneler(string uzmanlikAlani)
        {
            var hastaneler = GetHastanelerByUzmanlikAlani(uzmanlikAlani);
            return Json(hastaneler);
        }

        [HttpGet]
        public JsonResult GetDoktorlar(string uzmanlikAlani, string hastane)
        {
            var doktorlar = GetDoktorlarByUzmanlikAlaniVeHastane(uzmanlikAlani, hastane);
            return Json(doktorlar.Select(d => new SelectListItem
            {
                Value = d.DoktorID.ToString(),
                Text = d.Ad + " " + d.soyad
            }));
        }

        private List<SelectListItem> GetUzmanlikAlanlari()
        {
            List<SelectListItem> uzmanlikAlanlari = new List<SelectListItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DISTINCT UzmanlikAlani FROM Doktor";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            uzmanlikAlanlari.Add(new SelectListItem
                            {
                                Value = reader["UzmanlikAlani"].ToString(),
                                Text = reader["UzmanlikAlani"].ToString()
                            });
                        }
                    }
                }
            }
            return uzmanlikAlanlari;
        }

        private List<string> GetHastanelerByUzmanlikAlani(string uzmanlikAlani)
        {
            List<string> hastaneler = new List<string>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DISTINCT CalistigiHastane FROM Doktor WHERE UzmanlikAlani = @UzmanlikAlani";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UzmanlikAlani", uzmanlikAlani);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hastaneler.Add(reader["CalistigiHastane"].ToString());
                        }
                    }
                }
            }
            return hastaneler;
        }

        private List<Doktor> GetDoktorlarByUzmanlikAlaniVeHastane(string uzmanlikAlani, string calistigiHastane)
        {
            List<Doktor> doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad FROM Doktor WHERE UzmanlikAlani = @UzmanlikAlani AND CalistigiHastane = @CalistigiHastane";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@UzmanlikAlani", uzmanlikAlani);
                    command.Parameters.AddWithValue("@CalistigiHastane", calistigiHastane);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doktorlar.Add(new Doktor
                            {
                                DoktorID = (int)reader["DoktorID"],
                                Ad = reader["Ad"].ToString(),
                                soyad = reader["Soyad"].ToString()
                            });
                        }
                    }
                }
            }
            return doktorlar;
        }
        private string GetDoktorAdiById(int doktorId)
        {
            var doktor = GetDoktorById(doktorId);
            return doktor != null ? doktor.Ad : "";
        }
        private Doktor GetDoktorById(int doktorId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad, UzmanlikAlani FROM Doktor WHERE DoktorID = @DoktorID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoktorID", doktorId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Doktor
                            {
                                DoktorID = (int)reader["DoktorID"],
                                Ad = reader["Ad"].ToString(),
                                soyad = reader["Soyad"].ToString(),
                                UzmanlikAlani = reader["UzmanlikAlani"].ToString()
                            };
                        }
                        return null; // Doktor bulunamazsa null döndür
                    }
                }
            }
        }

        private List<Randevu> GetRandevular()
        {
            List<Randevu> randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT r.RandevuID, r.RandevuTarihi, r.RandevuSaati, r.HastaID, r.DoktorID, d.Ad as DoktorAdi FROM Randevu r INNER JOIN Doktor d ON r.DoktorID = d.DoktorID";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            randevular.Add(new Randevu
                            {
                                RandevuID = (int)reader["RandevuID"],
                                RandevuTarihi = (DateTime)reader["RandevuTarihi"],
                                RandevuSaati = ((TimeSpan)reader["RandevuSaati"]).ToString(),
                                HastaID = (int)reader["HastaID"],
                                DoktorID = (int)reader["DoktorID"],
                                //DoktorAdi = reader["DoktorAdi"].ToString() // Doktor adını ekle
                            });
                        }
                    }
                }
            }
            return randevular;
        }

        private void AddRandevu(Randevu randevu)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO Randevu (RandevuTarihi, RandevuSaati, HastaID, DoktorID) VALUES (@RandevuTarihi, @RandevuSaati, @HastaID, @DoktorID)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);
                    command.Parameters.AddWithValue("@HastaID", randevu.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                   // command.Parameters.AddWithValue("@DoktorAdi", randevu.DoktorAdi);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
        private List<SelectListItem> GetDoktorlar()
        {
            List<SelectListItem> doktorlar = new List<SelectListItem>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad FROM Doktor";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doktorlar.Add(new SelectListItem
                            {
                                Value = reader["DoktorID"].ToString(),
                                Text = reader["Ad"].ToString()
                            });
                        }
                    }
                }
            }
            return doktorlar;
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                DeleteRandevu(id);
                return RedirectToAction("Index", "Hasta");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Randevu silme işlemi sırasında bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private Randevu GetRandevuById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT RandevuID, RandevuTarihi, RandevuSaati, HastaID, DoktorID FROM Randevu WHERE RandevuID = @RandevuID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RandevuID", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Randevu
                            {
                                RandevuID = (int)reader["RandevuID"],
                                RandevuTarihi = (DateTime)reader["RandevuTarihi"],
                                RandevuSaati = ((TimeSpan)reader["RandevuSaati"]).ToString(),
                                HastaID = (int)reader["HastaID"],
                                DoktorID = (int)reader["DoktorID"]
                            };
                        }
                        return null; // Randevu bulunamazsa null döndür
                    }
                }
            }
        }

        private void UpdateRandevu(Randevu randevu)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
        UPDATE Randevu 
        SET 
            RandevuTarihi = @RandevuTarihi, 
            RandevuSaati = @RandevuSaati, 
            DoktorID = @DoktorID,
            DoktorAdi = @DoktorAdi
        WHERE 
            RandevuID = @RandevuID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RandevuID", randevu.RandevuID);
                    command.Parameters.AddWithValue("@RandevuTarihi", randevu.RandevuTarihi);
                    command.Parameters.AddWithValue("@RandevuSaati", randevu.RandevuSaati);
                    command.Parameters.AddWithValue("@DoktorID", randevu.DoktorID);
                   // command.Parameters.AddWithValue("@DoktorAdi", randevu.DoktorAdi);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Güncelleme işlemi başarısız. Randevu bulunamadı.");
                    }
                }
            }
        }



        private void DeleteRandevu(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM Randevu WHERE RandevuID = @RandevuID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RandevuID", id);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }
    }
}


