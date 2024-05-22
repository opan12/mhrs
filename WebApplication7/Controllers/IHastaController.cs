
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;
using Microsoft.Data.SqlClient;
using static Bogus.DataSets.Name;
using WebApplication7.Models;
public interface IHastaController
{
    List<Hasta> GetHastalar();
    Hasta GetHastaById(int id);

    void AddHasta(Hasta hasta);
    void UpdateHasta(Hasta hasta);
    void DeleteHasta(int hastaId);
}
