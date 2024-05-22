
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;
using Microsoft.Data.SqlClient;
using static Bogus.DataSets.Name;

public interface IDoktorController
{
    List<Doktor> GetDoktorlar();
    Doktor GetDoktorById(int id);
    List<Hasta> GetDoktorunHastalari(int doktorId);
    List<Randevu> GetDoktorunRandevulari(int doktorId);
    List<TibbiRapor> GetDoktorunHastalarininTibbiRaporlari(int doktorId);
    void AddDoktor(Doktor doktor);
    void UpdateDoktor(Doktor doktor);
    void DeleteDoktor(int doktorId);
}
