
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
    public class HastaService : IHastaController
    {
        private readonly string _connectionString;

        public HastaService(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        public List<Hasta> GetHastalar()
        {
            List<Hasta> hastalar = new List<Hasta>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM Hasta";
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

        public Hasta GetHastaById(int id)
        {
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT * FROM Hasta WHERE HastaID = @HastaID";
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

        public void AddHasta(Hasta hasta)
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

        public void UpdateHasta(Hasta hasta)
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

        public void DeleteHasta(int hastaId)
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
    }
}