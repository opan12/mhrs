
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;
using Microsoft.Data.SqlClient;
using static Bogus.DataSets.Name;

namespace WebApplication7.Controllers
{
    public class HastaController : Controller
    {
        private readonly string _connectionString;

        public HastaController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        [HttpGet]
        public IActionResult Index()
        {
            var hastaId = TempData["HastaID"] as int?;
            TempData["HastaID"] = hastaId;

            if (hastaId == null)
            {
                ViewBag.ErrorMessage = "Hasta kimliği bulunamadı.";
                return RedirectToAction("Index", "Home");
            }

            var hasta = GetHastaById(hastaId.Value);

            if (hasta == null)
            {
                return NotFound();
            }

            var randevular = GetRandevularByHastaId(hastaId.Value);
            var tibbiRaporlar = GetTibbiRaporlarByHastaId(hastaId.Value);

            ViewBag.Hasta = hasta;
            ViewBag.Randevular = randevular;
            ViewBag.TibbiRaporlar = tibbiRaporlar;


            return View(hasta);
        }
        [HttpGet]
        public IActionResult HastalarListesi()
        {
            List<Hasta> hastalar = GetHastalar();
            return View(hastalar);
        }

        [HttpGet]
        public IActionResult Details(int id)
        {
            var requestPage = HttpContext.Request.Query["page"].FirstOrDefault();
            ViewBag.RequestPage = requestPage != null ? Convert.ToInt32(requestPage) : 1;

            var hasta = GetHastaById(id);
            if (hasta == null)
            {
                return NotFound();
            }

            var tibbiRaporlar = GetTibbiRaporlarByHastaId(id);
            var randevular = GetHastaRandevulari(id);

            var model = new Tuple<Hasta, List<TibbiRapor>, List<Randevu>>(hasta, tibbiRaporlar, randevular);
            ViewBag.TibbiRaporlar = tibbiRaporlar;
            ViewBag.Randevular = randevular;
            return View(model);
        }

        private List<Randevu> GetHastaRandevulari(int hastaId)
        {
            List<Randevu> randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT RandevuID, RandevuTarihi, RandevuSaati, HastaID, DoktorID FROM Randevu WHERE HastaID = @HastaID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", hastaId);
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
                                DoktorID = (int)reader["DoktorID"]
                            });
                        }
                    }
                }
            }
            return randevular;
        }

        private List<TibbiRapor> GetTibbiRaporlarByHastaId(int hastaId)
        {
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM TibbiRapor WHERE HastaID = @HastaID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", hastaId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            raporlar.Add(new TibbiRapor
                            {
                                RaporID = (int)reader["RaporID"],
                                RaporTarihi = (DateTime)reader["RaporTarihi"],
                                RaporIcerigi = (string)reader["RaporIcerigi"],
                                RaporURL = (string)reader["RaporURL"],
                                HastaID = (int)reader["HastaID"],
                                DoktorID = (int)reader["DoktorID"]
                            });
                        }
                    }
                }
            }
            return raporlar;
        }
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var hasta = GetHastaById(id);

            if (hasta == null)
            {
                return NotFound();
            }

            return View(hasta);
        }
        [HttpGet]
        public IActionResult HastaListesi()
        {
            List<Hasta> hastalar = GetHastalar();
            return View(hastalar);
        }


        [HttpPost]
        public IActionResult Edit(Hasta hasta)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    UpdateHasta(hasta);
                    return RedirectToAction("HastaListesi");
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Hasta güncelleme işlemi sırasında bir hata oluştu: " + ex.Message;
                    return View(hasta);
                }
            }
            return View(hasta);
        }

        [HttpPost]
        public IActionResult Create(Hasta hasta)
        {
            try
            {
                AddHasta(hasta);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hasta ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }


        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                DeleteHasta(id);
                return RedirectToAction("HastaListesi");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Hasta silme işlemi sırasında bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        private Hasta GetHastaById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres FROM Hasta WHERE HastaID = @HastaID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Hasta
                            {
                                HastaID = (int)reader["HastaID"],
                                Ad = reader["Ad"] as string,
                                soyad = reader["Soyad"] as string,
                                DogumTarihi = reader["DogumTarihi"] as DateTime?,
                                Cinsiyet = reader["Cinsiyet"] as string,
                                TelefonNumarasi = reader["TelefonNumarasi"] as string,
                                Adres = reader["Adres"] as string
                            };
                        }
                        return null;
                    }
                }
            }
        }

        private List<Hasta> GetHastalar()
        {
            List<Hasta> hastalar = new List<Hasta>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT HastaID, Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres FROM Hasta";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hastalar.Add(new Hasta
                            {
                                HastaID = (int)reader["HastaID"],
                                Ad = reader["Ad"] as string,
                                soyad = reader["Soyad"] as string,
                                DogumTarihi = reader["DogumTarihi"] as DateTime?,
                                Cinsiyet = reader["Cinsiyet"] as string,
                                TelefonNumarasi = reader["TelefonNumarasi"] as string,
                                Adres = reader["Adres"] as string
                            });
                        }
                    }
                }
            }
            return hastalar;
        }

        private void AddHasta(Hasta hasta)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO Hasta (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, TC, Parola, Rol) VALUES (@Ad, @Soyad, @DogumTarihi, @Cinsiyet, @TelefonNumarasi, @Adres, @TC, @Parola, @Rol)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ad", hasta.Ad);
                    command.Parameters.AddWithValue("@Soyad", hasta.soyad);
                    command.Parameters.AddWithValue("@DogumTarihi", hasta.DogumTarihi ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Cinsiyet", hasta.Cinsiyet);
                    command.Parameters.AddWithValue("@TelefonNumarasi", hasta.TelefonNumarasi);
                    command.Parameters.AddWithValue("@Adres", hasta.Adres);
                    command.Parameters.AddWithValue("@TC", hasta.TC);
                    command.Parameters.AddWithValue("@Parola", hasta.Parola);
                    command.Parameters.AddWithValue("@Rol", 2);

                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        private void UpdateHasta(Hasta hasta)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            UPDATE Hasta 
            SET 
                Ad = @Ad, 
                Soyad = @Soyad, 
                DogumTarihi = @DogumTarihi, 
                Cinsiyet = @Cinsiyet,
                TelefonNumarasi = @TelefonNumarasi,
                Adres = @Adres
            WHERE 
                HastaID = @HastaID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", hasta.HastaID);
                    command.Parameters.AddWithValue("@Ad", hasta.Ad);
                    command.Parameters.AddWithValue("@Soyad", hasta.soyad);
                    command.Parameters.AddWithValue("@DogumTarihi", hasta.DogumTarihi ?? (object)DBNull.Value);
                    command.Parameters.AddWithValue("@Cinsiyet", hasta.Cinsiyet);
                    command.Parameters.AddWithValue("@TelefonNumarasi", hasta.TelefonNumarasi);
                    command.Parameters.AddWithValue("@Adres", hasta.Adres);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Güncelleme işlemi başarısız. Hasta bulunamadı.");
                    }
                }
            }
        }

        private void DeleteHasta(int hastaId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                // Transaction başlat
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        // Hasta ile ilişkili tıbbi raporları sil
                        var deleteTibbiRaporlarQuery = "DELETE FROM TibbiRapor WHERE HastaID = @HastaID";
                        using (var command = new SqlCommand(deleteTibbiRaporlarQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@HastaID", hastaId);
                            command.ExecuteNonQuery();
                        }

                        // Hasta ile ilişkili randevuları sil
                        var deleteRandevularQuery = "DELETE FROM Randevu WHERE HastaID = @HastaID";
                        using (var command = new SqlCommand(deleteRandevularQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@HastaID", hastaId);
                            command.ExecuteNonQuery();
                        }

                        // Hasta'yı sil
                        var deleteHastaQuery = "DELETE FROM Hasta WHERE HastaID = @HastaID";
                        using (var command = new SqlCommand(deleteHastaQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@HastaID", hastaId);
                            command.ExecuteNonQuery();
                        }

                        // Transaction commit
                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        // Transaction rollback
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }

        private List<Randevu> GetRandevularByHastaId(int hastaId)
        {
            List<Randevu> randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
            
   var query = @"
    SELECT r.RandevuID, r.RandevuTarihi, r.RandevuSaati, r.DoktorID, d.Ad as DoktorAdi
    FROM Randevu r
    INNER JOIN Doktor d ON r.DoktorID = d.DoktorID
    WHERE r.HastaID = @HastaID";


                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", hastaId);
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
                                DoktorID = (int)reader["DoktorID"],
                                //DoktorAdi = reader["DoktorAdi"] as string // DoktorAdi özelliğini doldur
                            });
                        }
                    }
                }
            }
            return randevular;
        }


    }
}
