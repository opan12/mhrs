/*using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models.EntityBase;
using Microsoft.Data.SqlClient;


namespace WebApplication7.Controllers
{
    public class FakeController : Controller
    {
        private readonly string _connectionString;

        public FakeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GenerateFakeData()
        {
            GenerateFakeHastaData();
            GenerateFakeDoktorData();
            GenerateFakeRandevuData();
            return Ok("Fake data generated successfully!");
        }

        private void GenerateFakeHastaData()
        {
            var faker = new Faker<Hasta>()
                .RuleFor(h => h.Ad, f => f.Person.FirstName)
                .RuleFor(h => h.soyad, f => f.Person.LastName)
                .RuleFor(h => h.DogumTarihi, f => f.Date.Past(70, DateTime.Now.AddYears(-18)))
                .RuleFor(h => h.Cinsiyet, f => f.PickRandom("Erkek", "Kadın"))
                .RuleFor(h => h.TelefonNumarasi, f =>
                {
                    var phone = f.Phone.PhoneNumber();
                    return phone.Length > 20 ? phone.Substring(0, 20) : phone;
                })
                .RuleFor(h => h.Adres, f => f.Address.FullAddress())
                .RuleFor(h => h.Parola, f => f.Internet.Password(8))

                .RuleFor(h => h.Rol, 2) // Hasta rolü
                .RuleFor(h => h.TC, f => GenerateRandomTC());

            var fakeHastalar = faker.Generate(10);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var fakeHasta in fakeHastalar)
                {
                    var query = "INSERT INTO Hasta (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Parola, Rol, TC) VALUES (@Ad, @Soyad, @DogumTarihi, @Cinsiyet, @TelefonNumarasi, @Adres, @Parola, @Rol, @TC)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ad", fakeHasta.Ad);
                        command.Parameters.AddWithValue("@Soyad", fakeHasta.soyad);
                        command.Parameters.AddWithValue("@DogumTarihi", fakeHasta.DogumTarihi);
                        command.Parameters.AddWithValue("@Cinsiyet", fakeHasta.Cinsiyet);
                        command.Parameters.AddWithValue("@TelefonNumarasi", fakeHasta.TelefonNumarasi);
                        command.Parameters.AddWithValue("@Adres", fakeHasta.Adres);
                        command.Parameters.AddWithValue("@Parola", fakeHasta.Parola);
                        command.Parameters.AddWithValue("@Rol", fakeHasta.Rol);
                        command.Parameters.AddWithValue("@TC", fakeHasta.TC);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        private void GenerateFakeRandevuData()
        {
            var doktorlar = GetDoktorlar();
            var hastalar = GetHastalar();

            var faker = new Faker<Randevu>()
                .RuleFor(r => r.RandevuTarihi, f => f.Date.Future())
                .RuleFor(r => r.RandevuSaati, f => f.Date.Recent().ToString("HH:mm"))
                .RuleFor(r => r.HastaID, f => f.PickRandom(hastalar).HastaID) // Rastgele bir hastayı seç
                .RuleFor(r => r.DoktorID, f => f.PickRandom(doktorlar).DoktorID); // Rastgele bir doktoru seç

            var fakeRandevular = faker.Generate(20); // 20 adet sahte randevu oluştur

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var fakeRandevu in fakeRandevular)
                {
                    var query = "INSERT INTO Randevu (RandevuTarihi, RandevuSaati, HastaID, DoktorID) VALUES (@RandevuTarihi, @RandevuSaati, @HastaID, @DoktorID)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@RandevuTarihi", fakeRandevu.RandevuTarihi);
                        command.Parameters.AddWithValue("@RandevuSaati", fakeRandevu.RandevuSaati);
                        command.Parameters.AddWithValue("@HastaID", fakeRandevu.HastaID);
                        command.Parameters.AddWithValue("@DoktorID", fakeRandevu.DoktorID);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }
        private void GenerateFakeDoktorData()
        {
            var faker = new Faker<Doktor>()
                .RuleFor(d => d.Ad, f => f.Person.FirstName)
                .RuleFor(d => d.soyad, f => f.Person.LastName)
                .RuleFor(d => d.UzmanlikAlani, f => f.Lorem.Word())
                .RuleFor(d => d.CalistigiHastane, f => f.Company.CompanyName())
                .RuleFor(d => d.Parola, f => f.Internet.Password(8)) // Şifreler en az 8, en fazla 50 karakter olacak
                .RuleFor(d => d.Rol, 1) // Doktor rolü
                .RuleFor(d => d.TC, f => GenerateRandomTC()); // Doktor için sahte T.C. numarası oluşturma

            var fakeDoktors = faker.Generate(5);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var fakeDoktor in fakeDoktors)
                {
                    var query = "INSERT INTO Doktor (Ad, Soyad, UzmanlikAlani, CalistigiHastane, Parola, Rol, TC) VALUES (@Ad, @Soyad, @UzmanlikAlani, @CalistigiHastane, @Parola, @Rol, @TC)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ad", fakeDoktor.Ad);
                        command.Parameters.AddWithValue("@Soyad", fakeDoktor.soyad);
                        command.Parameters.AddWithValue("@UzmanlikAlani", fakeDoktor.UzmanlikAlani);
                        command.Parameters.AddWithValue("@CalistigiHastane", fakeDoktor.CalistigiHastane);
                        command.Parameters.AddWithValue("@Parola", fakeDoktor.Parola);
                        command.Parameters.AddWithValue("@Rol", fakeDoktor.Rol);
                        command.Parameters.AddWithValue("@TC", fakeDoktor.TC);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private string GenerateRandomTC()
        {
            Random rand = new Random();
            string tc = "";
            for (int i = 0; i < 9; i++)
            {
                tc += rand.Next(1, 10).ToString();
            }
            int total10 = ((Convert.ToInt32(tc[0].ToString()) + Convert.ToInt32(tc[2].ToString()) + Convert.ToInt32(tc[4].ToString()) + Convert.ToInt32(tc[6].ToString()) + Convert.ToInt32(tc[8].ToString())) * 7 - (Convert.ToInt32(tc[1].ToString()) + Convert.ToInt32(tc[3].ToString()) + Convert.ToInt32(tc[5].ToString()) + Convert.ToInt32(tc[7].ToString()))) % 10;
            int total11 = 0;
            for (int i = 0; i < 9; i++)
            {
                total11 += Convert.ToInt32(tc[i].ToString());
            }
            total11 = total11 % 10;
            return tc + total10.ToString() + total11.ToString();
        }
    private List<Doktor> GetDoktorlar()
    {
        List<Doktor> doktorlar = new List<Doktor>();
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "SELECT DoktorID, Ad, Soyad FROM Doktor";
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
                            soyad = reader["Soyad"].ToString()
                        });
                    }
                }
            }
        }
        return doktorlar;
    }

    private List<Hasta> GetHastalar()
    {
        List<Hasta> hastalar = new List<Hasta>();
        using (var connection = new SqlConnection(_connectionString))
        {
            var query = "SELECT HastaID, Ad, Soyad FROM Hasta";
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
                            Ad = reader["Ad"].ToString(),
                            soyad = reader["Soyad"].ToString()
                        });
                    }
                }
            }
        }
        return hastalar;
    }

    }

}
*/
using Bogus;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models.EntityBase;
using Microsoft.Data.SqlClient;

namespace WebApplication7.Controllers
{
    public class FakeController : Controller
    {
        private readonly string _connectionString;

        public FakeController(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string is not configured.");
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GenerateFakeData()
        {
            GenerateFakeHastaData();
            GenerateFakeDoktorData();
            GenerateFakeRandevuData();
            return Ok("Fake data generated successfully!");
        }

        private void GenerateFakeHastaData()
        {
            var faker = new Faker<Hasta>()
                .RuleFor(h => h.Ad, f => f.Person.FirstName)
                .RuleFor(h => h.soyad, f => f.Person.LastName)
                .RuleFor(h => h.DogumTarihi, f => f.Date.Past(70, DateTime.Now.AddYears(-18)))
                .RuleFor(h => h.Cinsiyet, f => f.PickRandom("Erkek", "Kadın"))
                .RuleFor(h => h.TelefonNumarasi, f =>
                {
                    var phone = f.Phone.PhoneNumber();
                    return phone.Length > 20 ? phone.Substring(0, 20) : phone;
                })
                .RuleFor(h => h.Adres, f => f.Address.FullAddress())
                .RuleFor(h => h.Parola, f => f.Internet.Password(8))
                .RuleFor(h => h.Rol, 2) // Hasta rolü
                .RuleFor(h => h.TC, f => GenerateRandomTC());

            var fakeHastalar = faker.Generate(500);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var fakeHasta in fakeHastalar)
                {
                    var query = "INSERT INTO Hasta (Ad, Soyad, DogumTarihi, Cinsiyet, TelefonNumarasi, Adres, Parola, Rol, TC) VALUES (@Ad, @Soyad, @DogumTarihi, @Cinsiyet, @TelefonNumarasi, @Adres, @Parola, @Rol, @TC)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ad", fakeHasta.Ad);
                        command.Parameters.AddWithValue("@Soyad", fakeHasta.soyad);
                        command.Parameters.AddWithValue("@DogumTarihi", fakeHasta.DogumTarihi);
                        command.Parameters.AddWithValue("@Cinsiyet", fakeHasta.Cinsiyet);
                        command.Parameters.AddWithValue("@TelefonNumarasi", fakeHasta.TelefonNumarasi);
                        command.Parameters.AddWithValue("@Adres", fakeHasta.Adres);
                        command.Parameters.AddWithValue("@Parola", fakeHasta.Parola);
                        command.Parameters.AddWithValue("@Rol", fakeHasta.Rol);
                        command.Parameters.AddWithValue("@TC", fakeHasta.TC);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void GenerateFakeDoktorData()
        {
            var faker = new Faker<Doktor>()
                .RuleFor(d => d.Ad, f => f.Person.FirstName)
                .RuleFor(d => d.soyad, f => f.Person.LastName)
                .RuleFor(d => d.UzmanlikAlani, f => f.Lorem.Word())
                .RuleFor(d => d.CalistigiHastane, f => f.Company.CompanyName())
                .RuleFor(d => d.Parola, f => f.Internet.Password(8))
                .RuleFor(d => d.Rol, 1) // Doktor rolü
                .RuleFor(d => d.TC, f => GenerateRandomTC());

            var fakeDoktors = faker.Generate(50);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var fakeDoktor in fakeDoktors)
                {
                    var query = "INSERT INTO Doktor (Ad, Soyad, UzmanlikAlani, CalistigiHastane, Parola, Rol, TC) VALUES (@Ad, @Soyad, @UzmanlikAlani, @CalistigiHastane, @Parola, @Rol, @TC)";
                    using (var command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@Ad", fakeDoktor.Ad);
                        command.Parameters.AddWithValue("@Soyad", fakeDoktor.soyad);
                        command.Parameters.AddWithValue("@UzmanlikAlani", fakeDoktor.UzmanlikAlani);
                        command.Parameters.AddWithValue("@CalistigiHastane", fakeDoktor.CalistigiHastane);
                        command.Parameters.AddWithValue("@Parola", fakeDoktor.Parola);
                        command.Parameters.AddWithValue("@Rol", fakeDoktor.Rol);
                        command.Parameters.AddWithValue("@TC", fakeDoktor.TC);

                        command.ExecuteNonQuery();
                    }
                }
            }
        }

        private void GenerateFakeRandevuData()
        {
            var doktorlar = GetDoktorlar();
            var hastalar = GetHastalar();

            var faker = new Faker<Randevu>()
                .RuleFor(r => r.RandevuTarihi, f => f.Date.Future())
                .RuleFor(r => r.RandevuSaati, f => f.Date.Recent().ToString("HH:mm"))
                .RuleFor(r => r.HastaID, f => f.PickRandom(hastalar).HastaID)
                .RuleFor(r => r.DoktorID, f => f.PickRandom(doktorlar).DoktorID);

            using (var connection = new SqlConnection(_connectionString))
            {
                connection.Open();

                foreach (var hasta in hastalar)
                {
                    var fakeRandevular = faker.Generate(100); // Her hasta için 100 randevu oluştur

                    foreach (var fakeRandevu in fakeRandevular)
                    {
                        fakeRandevu.HastaID = hasta.HastaID; // Hasta ID'yi ayarla

                        var query = "INSERT INTO Randevu (RandevuTarihi, RandevuSaati, HastaID, DoktorID) VALUES (@RandevuTarihi, @RandevuSaati, @HastaID, @DoktorID)";
                        using (var command = new SqlCommand(query, connection))
                        {
                            command.Parameters.AddWithValue("@RandevuTarihi", fakeRandevu.RandevuTarihi);
                            command.Parameters.AddWithValue("@RandevuSaati", fakeRandevu.RandevuSaati);
                            command.Parameters.AddWithValue("@HastaID", fakeRandevu.HastaID);
                            command.Parameters.AddWithValue("@DoktorID", fakeRandevu.DoktorID);

                            command.ExecuteNonQuery();
                        }

                        // Her randevu oluşturulduğunda tıbbi rapor ekleyelim
                        GenerateFakeTibbiRaporData(fakeRandevu.HastaID, connection);
                    }
                }
            }
        }


        private void GenerateFakeTibbiRaporData(int hastaID, SqlConnection connection)
        {
            var urls = new List<string>
    {     "https://www.hekimoglugoruntuleme.com/wp-content/uploads/2022/04/tum-vucut-mr-960x400.jpg",
          "https://www.mumcu.com/wp-content/uploads/2014/03/5_hafta.jpg",
          "https://cdn-gomph.nitrocdn.com/ZffrWIXqhWQcDnEOzIkVkwnwuBeDhlKq/assets/images/optimized/rev-583916a/www.hekimoglugoruntuleme.com/wp-content/uploads/2017/01/mr-ultrason-arasindaki-farklar-nelerdir.png",
            "https://www.hekimoglugoruntuleme.com/wp-content/uploads/2022/03/kol-mr-700x400.jpg",
            "https://mrtomografi.com/wp-content/uploads/im001425.jpg",
            "https://www.hekimoglugoruntuleme.com/wp-content/uploads/2022/01/diz-mr-fiyatlari.png",
            "https://betatom.com.tr/wp-content/uploads/2023/07/aaddcbe9-4473-4627-8f85-8be4c7b052bc.jpg",
            "https://pbs.twimg.com/media/EqbP1hyXUAEqQUl.jpg",
            "https://galeri14.uludagsozluk.com/891/kan-tahlili_1232473.jpg",
            "https://tahlilsonucu.com/wp-content/uploads/2022/03/kan-tahlil-sonucu-ornegi.jpg",
            };

            var doktorlar = GetDoktorlar();

            var faker = new Faker<TibbiRapor>()
                .RuleFor(r => r.RaporTarihi, f => f.Date.Past(1))
                .RuleFor(r => r.RaporIcerigi, f => f.Lorem.Paragraph())
                .RuleFor(r => r.RaporURL, f => f.PickRandom(urls))
                .RuleFor(r => r.HastaID, f => hastaID)
                .RuleFor(r => r.DoktorID, f => f.PickRandom(doktorlar).DoktorID);

            var raporSayisi = new Random().Next(1, 2); // Her hastaya 1 ile 4 arası rapor ekle
            var fakeRaporlar = faker.Generate(raporSayisi);

            foreach (var fakeRapor in fakeRaporlar)
            {
                fakeRapor.HastaID = hastaID;

                var query = "INSERT INTO TibbiRapor (RaporTarihi, RaporIcerigi, RaporURL, HastaID, DoktorID) VALUES (@RaporTarihi, @RaporIcerigi, @RaporURL, @HastaID, @DoktorID)";
                using (var command = new SqlCommand(query, connection))
                {
                    command.Parameters.AddWithValue("@RaporTarihi", fakeRapor.RaporTarihi);
                    command.Parameters.AddWithValue("@RaporIcerigi", fakeRapor.RaporIcerigi);
                    command.Parameters.AddWithValue("@RaporURL", fakeRapor.RaporURL);
                    command.Parameters.AddWithValue("@HastaID", fakeRapor.HastaID);
                    command.Parameters.AddWithValue("@DoktorID", fakeRapor.DoktorID);

                    command.ExecuteNonQuery();
                }
            }
        }

        private string GenerateRandomTC()
        {
            Random rand = new Random();
            string tc = "";
            for (int i = 0; i < 9; i++)
            {
                tc += rand.Next(1, 10).ToString();
            }
            int total10 = ((Convert.ToInt32(tc[0].ToString()) + Convert.ToInt32(tc[2].ToString()) + Convert.ToInt32(tc[4].ToString()) + Convert.ToInt32(tc[6].ToString()) + Convert.ToInt32(tc[8].ToString())) * 7 - (Convert.ToInt32(tc[1].ToString()) + Convert.ToInt32(tc[3].ToString()) + Convert.ToInt32(tc[5].ToString()) + Convert.ToInt32(tc[7].ToString()))) % 10;
            int total11 = 0;
            for (int i = 0; i < 9; i++)
            {
                total11 += Convert.ToInt32(tc[i].ToString());
            }
            total11 = total11 % 10;
            return tc + total10.ToString() + total11.ToString();
        }

        private List<Doktor> GetDoktorlar()
        {
            List<Doktor> doktorlar = new List<Doktor>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT DoktorID, Ad, Soyad FROM Doktor";
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
                                soyad = reader["Soyad"].ToString()
                            });
                        }
                    }
                }
            }
            return doktorlar;
        }

        private List<Hasta> GetHastalar()
        {
            List<Hasta> hastalar = new List<Hasta>();
            using (var connection = new SqlConnection(_connectionString))
            {
                var query = "SELECT HastaID, Ad, Soyad FROM Hasta";
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
                                Ad = reader["Ad"].ToString(),
                                soyad = reader["Soyad"].ToString()
                            });
                        }
                    }
                }
            }
            return hastalar;
        }
    }
}
