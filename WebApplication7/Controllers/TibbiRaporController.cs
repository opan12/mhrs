using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using WebApplication7.Models.EntityBase;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace WebApplication7.Controllers
{
    public class TibbiRaporController : Controller
    {
        private readonly string _connectionString;
        private readonly IDoktorController _doktorService;

        public TibbiRaporController(IConfiguration configuration, IDoktorController _doktorService)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
            _doktorService = _doktorService;
        }

    
      

        [HttpGet]
        public IActionResult Index()
        {
            List<Doktor> doktorlar = _doktorService.GetDoktorlar();

            List<TibbiRapor> raporlar = GetTibbiRaporlar();
            return View(raporlar);
        }

        [HttpGet]
        public IActionResult Create(int hastaId)
        {
            if (TempData["DoktorID"] == null)
            {
                return Unauthorized(); // Eğer doktor ID'si TempData'da yoksa yetkisiz erişim
            }

            var doktorId = (int)TempData["DoktorID"];
            TempData.Keep("DoktorID"); // TempData'daki veriyi sonraki istekler için sakla

            var model = new TibbiRapor { HastaID = hastaId, DoktorID = doktorId};
            ViewBag.DoktorID = doktorId;
            return View();
        }
        [HttpPost]
        public IActionResult Create(TibbiRapor rapor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    rapor.DoktorID = (int)TempData["DoktorID"];
                    TempData.Keep("DoktorID"); // TempData'daki veriyi sonraki istekler için sakla

                    AddTibbiRapor(rapor);
                    var notification = new Notification
                    {
                        Message = "Yeni laboratuvar sonucu yüklendi.",
                        HastaID = rapor.HastaID,
                        Timestamp=rapor.RaporTarihi
                        
                    };
                    AddNotification(notification);
                    return RedirectToAction("Details", "Hasta", new { id = rapor.HastaID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Tıbbi rapor ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
            }
            return View(rapor);
        }
        private void AddTibbiRapor(TibbiRapor rapor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "INSERT INTO TibbiRapor (RaporTarihi, RaporIcerigi, RaporURL, HastaID, DoktorID) VALUES (@RaporTarihi, @RaporIcerigi, @RaporURL, @HastaID, @DoktorID)";
                using (var command = new SqlCommand(query, connection))
                {
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
        private TibbiRapor GetTibbiRaporById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT RaporID, RaporTarihi, RaporIcerigi, RaporURL, HastaID, DoktorID FROM TibbiRapor WHERE RaporID = @RaporID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RaporID", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TibbiRapor
                            {
                                RaporID = (int)reader["RaporID"],
                                RaporTarihi = (DateTime)reader["RaporTarihi"],
                                RaporIcerigi = (string)reader["RaporIcerigi"],
                                RaporURL = (string)reader["RaporURL"],
                                HastaID = (int)reader["HastaID"],
                                DoktorID = (int)reader["DoktorID"]
                            };
                        }
                    }
                }
            }
            return null;
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
        [HttpGet]
        public IActionResult Details(int id)
        {
            var rapor = GetTibbiRaporById(id);
            if (rapor == null)
            {
                return NotFound();
            }
            return View(rapor);
        }
     
        private List<Doktor> GetDoktorlar()
        {
            List<Doktor> doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad, UzmanlikAlani FROM Doktor";
                using (var command = new SqlCommand(query, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            doktorlar.Add(new Doktor
                            {
                                DoktorID = (int)reader["DoktorID"],
                                Ad = reader["Ad"].ToString(),
                                soyad = reader["Soyad"].ToString(),
                                UzmanlikAlani = reader["UzmanlikAlani"].ToString()
                            });
                        }
                    }
                }
            }
            ViewBag.Doktorlar = new SelectList(doktorlar, "DoktorID", "Ad");
            return doktorlar;
        }
        [HttpPost]
        public IActionResult Delete(int raporId, int hastaId)
        {
            try
            {
                DeleteTibbiRapor(raporId);
                // Silme işlemi başarılı olduğunda, JavaScript kodunu kullanarak sayfayı yenileyelim
                return Ok(); // Başarı durumunda 200 OK yanıtı döndürür
            }
            catch (Exception ex)
            {
                // Hata durumunda bir hata iletisi döndürelim
                return BadRequest("Tıbbi rapor silme işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult Upload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult Upload(TibbiRapor rapor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    if (rapor.File != null && rapor.File.Length > 0)
                    {
                        var dosyaAdi = Path.GetFileName(rapor.File.FileName);
                        var benzersizDosyaAdi = Guid.NewGuid().ToString() + Path.GetExtension(dosyaAdi);
                        var dosyaYolu = Path.Combine("uploads", benzersizDosyaAdi);

                        using (var stream = new FileStream(Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", dosyaYolu), FileMode.Create))
                        {
                            rapor.File.CopyTo(stream);
                        }

                        rapor.RaporURL = dosyaYolu;
                    }
                    else
                    {
                        rapor.RaporURL = "";
                    }

                    AddTibbiRapor(rapor);

                    return RedirectToAction("Details", "Hasta", new { id = rapor.HastaID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Tıbbi rapor ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
            }
            return View(rapor);
        }


        [HttpGet]
        public async Task<IActionResult> DownloadFromUrl(string fileUrl)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    using (var response = await client.GetAsync(fileUrl))
                    {
                        if (response.IsSuccessStatusCode)
                        {
                            var fileBytes = await response.Content.ReadAsByteArrayAsync();
                            return File(fileBytes, "application/octet-stream");
                        }
                        else
                        {
                            return NotFound("Dosya indirme işlemi sırasında bir hata oluştu. Dosya bulunamadı.");
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                return BadRequest("Dosya indirme işlemi sırasında bir hata oluştu: " + ex.Message);
            }
        }


        [HttpGet]
        public IActionResult DownloadFileFromUrl()
        {
            return View();
        }
      
        private List<TibbiRapor> GetTibbiRaporlar()
        {
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT r.RaporID, r.RaporTarihi, r.RaporIcerigi,rRaporURL, r.HastaID, r.DoktorID,  as  FROM Rapor r INNER JOIN Doktor d ON r.DoktorID = d.DoktorID";
                using (var command = new SqlCommand(query, connection))
                {
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
                                DoktorID = (int)reader["DoktorID"],
                              
                            });
                        }
                    }
                }
            }
            return raporlar;
        }



        private void DeleteTibbiRapor(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "DELETE FROM TibbiRapor WHERE RaporID = @RaporID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RaporID", id);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        [HttpGet]
        public IActionResult Notification()
        {
            var hastaId = TempData["HastaID"] as int?;
            var notifications = GetNotifications(hastaId.Value);

            // Pass notifications to the view
            return View(notifications);
        }


        [HttpGet]
    
        public IActionResult Edit(int id, int hastaId)
        {
            var rapor = GetRaporById(id);
            if (rapor == null)
            {
                return NotFound();
            }
            rapor.HastaID = hastaId; // HastaID'yi modeli ayarlayın
            return View(rapor);
        }

        [HttpPost]
        public IActionResult Edit(TibbiRapor rapor)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    UpdateRapor(rapor);
                    var notification = new Notification
                    {
                        Message = "Yeni laboratuvar sonucu yüklendi.",
                        HastaID = rapor.HastaID,
                        Timestamp=rapor.RaporTarihi

                    };
                    AddNotification(notification);
                    return RedirectToAction("Details", "Hasta", new { id = rapor.HastaID });
                }
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Rapor güncelleme işlemi sırasında bir hata oluştu: " + ex.Message;
            }

            return View(rapor);

        }

        private void UpdateRapor(TibbiRapor rapor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
            UPDATE TibbiRapor 
            SET 
                RaporTarihi = @RaporTarihi, 
                RaporIcerigi = @RaporIcerigi, 
                RaporURL = @RaporURL
            WHERE 
                RaporID = @RaporID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RaporID", rapor.RaporID);
                    command.Parameters.AddWithValue("@RaporTarihi", rapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", rapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@RaporURL", rapor.RaporURL);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Güncelleme işlemi başarısız. Rapor bulunamadı.");
                    }
                }
            }
        }

        private TibbiRapor GetRaporById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT RaporID, RaporTarihi, RaporIcerigi, RaporURL FROM TibbiRapor WHERE RaporID = @RaporID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RaporID", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new TibbiRapor
                            {
                                RaporID = (int)reader["RaporID"],
                                RaporTarihi = (DateTime)reader["RaporTarihi"],
                                RaporIcerigi = reader["RaporIcerigi"].ToString(),
                                RaporURL = reader["RaporURL"].ToString()
                            };
                        }
                        return null; // Rapor bulunamazsa null döndür
                    }
                }
            }
        }
    

    private void AddNotification(Notification notification)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "INSERT INTO Notification (Message, HastaID,Timestamp) VALUES (@Message, @HastaID,@Timestamp)";
            using (var command = new SqlCommand(query, connection))
            {
                command.Parameters.AddWithValue("@Message", notification.Message);
                    command.Parameters.AddWithValue("@HastaID", notification.HastaID);
                    command.Parameters.AddWithValue("@Timestamp", notification.Timestamp);
                    connection.Open();
                command.ExecuteNonQuery();
            }


        } }
        private List<Notification> GetNotifications(int hastaId)
        {
            List<Notification> notifications = new List<Notification>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT NotificationID, Timestamp, Message, HastaID FROM Notification WHERE HastaID = @HastaID ORDER BY Timestamp DESC";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@HastaID", hastaId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            notifications.Add(new Notification
                            {
                                NotificationID = reader["NotificationID"] != DBNull.Value ? (int)reader["NotificationID"] : 0,
                                Timestamp = reader["Timestamp"] != DBNull.Value ? (DateTime)reader["Timestamp"] : DateTime.MinValue,
                                Message = reader["Message"] != DBNull.Value ? reader["Message"].ToString() : string.Empty,
                                HastaID = reader["HastaID"] != DBNull.Value ? (int)reader["HastaID"] : 0,
                            });
                        }
                    }
                }
            }
            return notifications;
        }



    }
}

