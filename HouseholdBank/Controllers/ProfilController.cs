using HouseholdBank.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseholdBank.Controllers
{
    public class ProfilController : Controller
    {
        // GET: Profil
        public ActionResult Index()
        {
            if (Session["mus"]!=null)
            {
                return View();
            }
            return RedirectToAction("Index", "Login");
        }
        
        // GET: Profil/Duzenle
        public ActionResult Duzenle()
        {
            if (Session["mus"]==null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        [HttpPost]
        public ActionResult Duzenle(Musteri m)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            Musteri tempMus = new Musteri();

            dbBankEntities db = new dbBankEntities();
            tempMus = db.Musteri.Single(mus => mus.tcKimlikNo == m.tcKimlikNo);

            tempMus.tcKimlikNo = m.tcKimlikNo;
            tempMus.tamAdi = m.tamAdi;
            tempMus.sifre = m.sifre;
            tempMus.adres = m.adres;
            tempMus.telefon = m.telefon;
            tempMus.dogumTarihi = m.dogumTarihi;
            tempMus.ePosta = m.ePosta;

            db.Musteri.Attach(tempMus);
            db.Entry(tempMus).State = EntityState.Modified;
            db.SaveChanges();

            Session["mus"] = tempMus;

            return RedirectToAction("Index", "Login");
        }
    }
}