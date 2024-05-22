using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;
using WebApplication7.Controllers;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;
namespace WebApplication7.Controllers
{
    public class DoktorService : IDoktorController
    {
        private readonly string _connectionString;

        public DoktorService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        public List<Doktor> GetDoktorlar()
        {
            List<Doktor> doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane FROM Doktor";
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
                                Ad = (string)reader["Ad"],
                                soyad = (string)reader["Soyad"],
                                UzmanlikAlani = (string)reader["UzmanlikAlani"],
                                CalistigiHastane = (string)reader["CalistigiHastane"]
                            });
                        }
                    }
                }
            }
            return doktorlar;
        }

        public Doktor GetDoktorById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad, UzmanlikAlani, CalistigiHastane, TC, Parola FROM Doktor WHERE DoktorID = @DoktorID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoktorID", id);
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
                                UzmanlikAlani = reader["UzmanlikAlani"].ToString(),
                                CalistigiHastane = reader["CalistigiHastane"].ToString(),
                                TC = reader["TC"].ToString(),
                                Parola = reader["Parola"].ToString()
                            };
                        }
                        return null;
                    }
                }
            }
        }

        public List<Hasta> GetDoktorunHastalari(int doktorId)
        {
            List<Hasta> hastalar = new List<Hasta>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM Hasta WHERE HastaID IN (SELECT HastaID FROM Randevu WHERE DoktorID = @DoktorID)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoktorID", doktorId);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            hastalar.Add(new Hasta
                            {
                                HastaID = (int)reader["HastaID"],
                                Ad = (string)reader["Ad"],
                                soyad = (string)reader["Soyad"],
                                Cinsiyet = (string)reader["Cinsiyet"],
                                DogumTarihi = reader["DogumTarihi"] != DBNull.Value ? (DateTime)reader["DogumTarihi"] : (DateTime?)null,
                                TC = (string)reader["TC"],
                                TelefonNumarasi = (string)reader["TelefonNumarasi"],
                                Adres = (string)reader["Adres"]
                            });
                        }
                    }
                }
            }
            return hastalar;
        }

        public List<Randevu> GetDoktorunRandevulari(int doktorId)
        {
            List<Randevu> randevular = new List<Randevu>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT RandevuID, RandevuTarihi, RandevuSaati, HastaID, DoktorID FROM Randevu WHERE DoktorID = @DoktorID";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoktorID", doktorId);
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

        public List<TibbiRapor> GetDoktorunHastalarininTibbiRaporlari(int doktorId)
        {
            List<TibbiRapor> raporlar = new List<TibbiRapor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var hastalarQuery = "SELECT HastaID FROM Randevu WHERE DoktorID = @DoktorID";
                using (var hastalarCommand = new SqlCommand(hastalarQuery, connection))
                {
                    hastalarCommand.Parameters.AddWithValue("@DoktorID", doktorId);
                    connection.Open();
                    using (var hastalarReader = hastalarCommand.ExecuteReader())
                    {
                        while (hastalarReader.Read())
                        {
                            int hastaId = (int)hastalarReader["HastaID"];
                            var raporlarQuery = "SELECT RaporID, RaporTarihi, RaporIcerigi, RaporURL FROM TibbiRapor WHERE HastaID = @HastaID";
                            using (var raporlarCommand = new SqlCommand(raporlarQuery, connection))
                            {
                                raporlarCommand.Parameters.AddWithValue("@HastaID", hastaId);
                                using (var raporlarReader = raporlarCommand.ExecuteReader())
                                {
                                    while (raporlarReader.Read())
                                    {
                                        raporlar.Add(new TibbiRapor
                                        {
                                            RaporID = (int)raporlarReader["RaporID"],
                                            RaporTarihi = (DateTime)raporlarReader["RaporTarihi"],
                                            RaporIcerigi = (string)raporlarReader["RaporIcerigi"],
                                            RaporURL = (string)raporlarReader["RaporURL"]
                                        });
                                    }
                                }
                            }
                        }
                    }
                }
            }
            return raporlar;
        }

        public void AddDoktor(Doktor doktor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                INSERT INTO Doktor 
                    (Ad, Soyad, UzmanlikAlani, CalistigiHastane, TC, Parola, Rol) 
                VALUES 
                    (@Ad, @Soyad, @UzmanlikAlani, @CalistigiHastane, @TC, @Parola, @Rol)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@Ad", doktor.Ad);
                    command.Parameters.AddWithValue("@Soyad", doktor.soyad);
                    command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                    command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                    command.Parameters.AddWithValue("@TC", doktor.TC);
                    command.Parameters.AddWithValue("@Parola", doktor.Parola);
                    command.Parameters.AddWithValue("@Rol", 1);
                    connection.Open();
                    command.ExecuteNonQuery();
                }
            }
        }

        public void UpdateDoktor(Doktor doktor)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = @"
                UPDATE Doktor 
                SET 
                    Ad = @Ad, 
                    Soyad = @Soyad, 
                    UzmanlikAlani = @UzmanlikAlani, 
                    CalistigiHastane = @CalistigiHastane,
                    TC = @TC,
                    Parola = @Parola,
                    Rol = @Rol
                WHERE 
                    DoktorID = @DoktorID";

                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@DoktorID", doktor.DoktorID);
                    command.Parameters.AddWithValue("@Ad", doktor.Ad);
                    command.Parameters.AddWithValue("@Soyad", doktor.soyad);
                    command.Parameters.AddWithValue("@UzmanlikAlani", doktor.UzmanlikAlani);
                    command.Parameters.AddWithValue("@CalistigiHastane", doktor.CalistigiHastane);
                    command.Parameters.AddWithValue("@TC", doktor.TC);
                    command.Parameters.AddWithValue("@Parola", doktor.Parola);
                    command.Parameters.AddWithValue("@Rol", 1);

                    connection.Open();
                    int rowsAffected = command.ExecuteNonQuery();

                    if (rowsAffected == 0)
                    {
                        throw new Exception("Güncelleme işlemi başarısız. Doktor bulunamadı.");
                    }
                }
            }
        }

        public void DeleteDoktor(int doktorId)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();
                using (var transaction = connection.BeginTransaction())
                {
                    try
                    {
                        var deleteTibbiRaporlarQuery = "DELETE FROM TibbiRapor WHERE DoktorID = @DoktorID";
                        using (var command = new SqlCommand(deleteTibbiRaporlarQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DoktorID", doktorId);
                            command.ExecuteNonQuery();
                        }

                        var deleteRandevularQuery = "DELETE FROM Randevu WHERE DoktorID = @DoktorID";
                        using (var command = new SqlCommand(deleteRandevularQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DoktorID", doktorId);
                            command.ExecuteNonQuery();
                        }

                        var deleteDoktorQuery = "DELETE FROM Doktor WHERE DoktorID = @DoktorID";
                        using (var command = new SqlCommand(deleteDoktorQuery, connection, transaction))
                        {
                            command.Parameters.AddWithValue("@DoktorID", doktorId);
                            command.ExecuteNonQuery();
                        }

                        transaction.Commit();
                    }
                    catch (Exception)
                    {
                        transaction.Rollback();
                        throw;
                    }
                }
            }
        }
    }
}