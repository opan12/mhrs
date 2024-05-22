using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using WebApplication7.Models;
using WebApplication7.Models.EntityBase;

namespace WebApplication7.Controllers
{
    public class DoktorController : Controller
    {
        private readonly IDoktorController _doktorService;

        public DoktorController(IDoktorController doktorService)
        {
            _doktorService = doktorService;
        }

        [HttpGet]
        public IActionResult DoktorListesiView()
        {
            var doktorlar = _doktorService.GetDoktorlar();
            return View("DoktorListesiView", doktorlar);
        }

        [HttpGet]
        public IActionResult Index()
        {
            if (TempData["DoktorID"] is int doktorId)
            {
                TempData["DoktorID"] = doktorId;

                List<Hasta> doktorunHastalari = _doktorService.GetDoktorunHastalari(doktorId);
                List<Randevu> doktorunRandevulari = _doktorService.GetDoktorunRandevulari(doktorId);
                List<TibbiRapor> doktorunTibbiRaporlari = _doktorService.GetDoktorunHastalarininTibbiRaporlari(doktorId);

                var model = new Tuple<List<Doktor>, List<Hasta>, List<Randevu>, List<TibbiRapor>>(
                    _doktorService.GetDoktorlar(),
                    doktorunHastalari,
                    doktorunRandevulari,
                    doktorunTibbiRaporlari
                );

                return View(model);
            }
            else
            {
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Delete()
        {
            return View();
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var doktor = _doktorService.GetDoktorById(id);
            if (doktor == null)
            {
                return NotFound();
            }
            return View(doktor);
        }

        [HttpPost]
        public IActionResult Create(Doktor doktor)
        {
            try
            {
                _doktorService.AddDoktor(doktor);
                return RedirectToAction("Index");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Doktor ekleme işlemi sırasında bir hata oluştu: " + ex.Message;
                return View();
            }
        }

        [HttpPost]
        public IActionResult Delete(int id)
        {
            try
            {
                _doktorService.DeleteDoktor(id);
                return RedirectToAction("DoktorListesiView");
            }
            catch (Exception ex)
            {
                ViewBag.ErrorMessage = "Doktor silme işlemi sırasında bir hata oluştu: " + ex.Message;
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public IActionResult Edit(Doktor doktor)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _doktorService.UpdateDoktor(doktor);
                    return RedirectToAction("DoktorListesiView");
                }
                catch (Exception ex)
                {
                    ViewBag.ErrorMessage = "Doktor güncelleme işlemi sırasında bir hata oluştu: " + ex.Message;
                    return View(doktor);
                }
            }
            else
            {
                foreach (var entry in ModelState)
                {
                    if (entry.Value.Errors.Count > 0)
                    {
                        Console.WriteLine($"{entry.Key} : {entry.Value.Errors[0].ErrorMessage}");
                    }
                }
            }
            return View(doktor);
        }
    }
}
