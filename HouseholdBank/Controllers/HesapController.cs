using HouseholdBank.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace HouseholdBank.Controllers
{
    public class HesapController : Controller
    {
        // GET: Hesap
        public ActionResult Index()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");
            return View();
        }


        public ActionResult Detay(int id)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == id.ToString() && h.aktifmi == true).Single();

            if (secilenHesap != null)
            {
                ViewBag.hesap = secilenHesap;
            }
            return View();
        }

        public ActionResult HesapKapat(int id)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == id.ToString() && h.aktifmi == true).Single();

            secilenHesap.aktifmi = false;
            secilenHesap.kapanisTarihi = DateTime.Now;
            db.Hesap.Attach(secilenHesap);
            db.Entry(secilenHesap).State = EntityState.Modified;
            db.SaveChanges();

            return RedirectToAction("Index", "Hesap");
        }

        [HttpPost]
        public ActionResult ParaYatir(int id, FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == id.ToString() && h.aktifmi == true).Single();

            int yatirilacakMiktar = Convert.ToInt32(fc["miktar"]);

            secilenHesap.bakiye += yatirilacakMiktar;
            db.Hesap.Attach(secilenHesap);
            db.Entry(secilenHesap).State = EntityState.Modified;

            Islem islem = new Islem();
            islem.hesapNo = mus.hesapNo;
            islem.hesapEkNo = secilenHesap.hesapEkNo;
            islem.islemTipi = "Para Yatırma";
            islem.tarih = DateTime.Now;
            islem.aciklama = yatirilacakMiktar + " TL Para Yatırıldı - " + fc["aciklama"];

            db.Islem.Add(islem);
            db.SaveChanges();

            int gelenID = id;
            return RedirectToAction("Detay", "Hesap", new { id = gelenID });
        }

        [HttpPost]
        public ActionResult ParaCek(int id, FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == id.ToString() && h.aktifmi == true).Single();

            int cekilecekMiktar = Convert.ToInt32(fc["miktar"]);

            secilenHesap.bakiye -= cekilecekMiktar;
            db.Hesap.Attach(secilenHesap);
            db.Entry(secilenHesap).State = EntityState.Modified;

            Islem islem = new Islem();
            islem.hesapNo = mus.hesapNo;
            islem.hesapEkNo = secilenHesap.hesapEkNo;
            islem.islemTipi = "Para Çekme";
            islem.tarih = DateTime.Now;
            islem.aciklama = cekilecekMiktar + " TL Para Çekildi - " + fc["aciklama"];

            db.Islem.Add(islem);
            db.SaveChanges();

            int gelenID = id;
            return RedirectToAction("Detay", "Hesap", new { id = gelenID });
        }

        public ActionResult YeniHesap()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            Hesap yeniHesap = new Hesap();

            yeniHesap.bakiye = 0;
            yeniHesap.musTCKN = mus.tcKimlikNo;
            yeniHesap.aktifmi = true;
            yeniHesap.acilisTarihi = DateTime.Now;


            List<Hesap> hesapList = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo).ToList();
            int enSonHesap = 0;
            foreach (Hesap hesap in hesapList)
            {
                if (enSonHesap< Convert.ToInt32(hesap.hesapEkNo))
                {
                    enSonHesap = Convert.ToInt32(hesap.hesapEkNo);
                }
            }
            yeniHesap.hesapEkNo = (enSonHesap + 1).ToString();

            db.Hesap.Add(yeniHesap);
            db.SaveChanges();

            return RedirectToAction("Index", "Hesap");
        }

    }
}