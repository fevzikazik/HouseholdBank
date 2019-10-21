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

        public ActionResult VirmanGonderen()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        [HttpPost]
        public ActionResult VirmanAlan(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            
            string gonderenHesap = fc.Get("gonderenHesap");
            ViewBag.gonderenHesap = gonderenHesap;

            return View();
        }

        [HttpPost]
        public ActionResult Virman(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            
            string gonderenHesap = fc.Get("gonderenHesap");
            string alanHesap = fc.Get("alanHesap");
            ViewBag.gonderenHesap = gonderenHesap;
            ViewBag.alanHesap = alanHesap;

            return View();
        }

        [HttpPost]
        public ActionResult VirmanKontrol(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            string gonderenHesapEkNo = fc.Get("gonderenHesap");
            string alanHesapEkNo = fc.Get("alanHesap");
            int virmanMiktar = Convert.ToInt32(fc.Get("miktar"));

            Hesap gonderenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == gonderenHesapEkNo && h.aktifmi == true).Single();
            Hesap alanHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == alanHesapEkNo && h.aktifmi == true).Single();

            gonderenHesap.bakiye -= virmanMiktar;
            alanHesap.bakiye += virmanMiktar;
            db.Hesap.Attach(gonderenHesap);
            db.Hesap.Attach(alanHesap);
            db.Entry(gonderenHesap).State = EntityState.Modified;
            db.Entry(alanHesap).State = EntityState.Modified;

            Islem gonderenIslem = new Islem();
            gonderenIslem.hesapNo = mus.hesapNo;
            gonderenIslem.hesapEkNo = gonderenHesap.hesapEkNo;
            gonderenIslem.islemTipi = "Virman";
            gonderenIslem.tarih = DateTime.Now;
            gonderenIslem.aciklama = virmanMiktar + " TL " + alanHesapEkNo + " EkNolu Hesaba Virman Yapildi!";
            db.Islem.Add(gonderenIslem);

            Islem alanIslem = new Islem();
            alanIslem.hesapNo = mus.hesapNo;
            alanIslem.hesapEkNo = alanHesap.hesapEkNo;
            alanIslem.islemTipi = "Virman";
            alanIslem.tarih = DateTime.Now;
            alanIslem.aciklama = virmanMiktar + " TL " + gonderenHesapEkNo + " EkNolu Hesabtan Virman Geldi!";
            db.Islem.Add(alanIslem);

            db.SaveChanges();
            
            ViewBag.gonderenHesap = gonderenHesap;
            ViewBag.alanHesap = alanHesap;
            ViewBag.miktar = virmanMiktar;


            return View();
        }

        public ActionResult HavaleGonderen()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            return View();
        }

        [HttpPost]
        public ActionResult HavaleAlan(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            
            string gonderenHesapEkNo = fc.Get("gonderenHesapEkNo");
            ViewBag.gonderenHesapEkNo = gonderenHesapEkNo;

            return View();
        }

        [HttpPost]
        public ActionResult Havale(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            
            string gonderenHesapEkNo = fc.Get("gonderenHesapEkNo");
            string aliciHesap = fc.Get("aliciHesapNo");
            string aliciHesapEkNo = fc.Get("aliciHesapEkNo");
            ViewBag.gonderenHesapEkNo = gonderenHesapEkNo;
            ViewBag.aliciHesap = aliciHesap;
            ViewBag.aliciHesapEkNo = aliciHesapEkNo;

            return View();
        }

        [HttpPost]
        public ActionResult HavaleKontrol(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();

            Musteri mus = (Musteri)Session["mus"];

            string gonderenHesapEkNo = fc.Get("gonderenHesapEkNo");
            string aliciHesap = fc.Get("aliciHesap");
            string aliciHesapEkNo = fc.Get("aliciHesapEkNo");
            int havaleMiktar = Convert.ToInt32(fc.Get("miktar"));

            Musteri aliciMus = db.Musteri.Where(m => m.hesapNo == aliciHesap).Single();

            Hesap gonderenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == gonderenHesapEkNo && h.aktifmi == true).Single();
            Hesap alanHesap = db.Hesap.Where(h => h.musTCKN == aliciMus.tcKimlikNo && h.hesapEkNo == aliciHesapEkNo && h.aktifmi == true).Single();

            gonderenHesap.bakiye -= havaleMiktar;
            alanHesap.bakiye += havaleMiktar;
            db.Hesap.Attach(gonderenHesap);
            db.Hesap.Attach(alanHesap);
            db.Entry(gonderenHesap).State = EntityState.Modified;
            db.Entry(alanHesap).State = EntityState.Modified;

            Islem gonderenIslem = new Islem();
            gonderenIslem.hesapNo = mus.hesapNo;
            gonderenIslem.hesapEkNo = gonderenHesap.hesapEkNo;
            gonderenIslem.islemTipi = "Havale";
            gonderenIslem.tarih = DateTime.Now;
            gonderenIslem.aciklama = havaleMiktar + " TL " + aliciHesap + " Nolu Müşterinin " + aliciHesapEkNo  + " EkNolu Hesabına Havale Yapildi!";
            db.Islem.Add(gonderenIslem);

            Islem alanIslem = new Islem();
            alanIslem.hesapNo = aliciMus.hesapNo;
            alanIslem.hesapEkNo = alanHesap.hesapEkNo;
            alanIslem.islemTipi = "Havale";
            alanIslem.tarih = DateTime.Now;
            alanIslem.aciklama = havaleMiktar + " TL " + mus.hesapNo + " Nolu Müşteriden " + gonderenHesapEkNo + " EkNolu Hesabından Havale Geldi!";
            db.Islem.Add(alanIslem);

            db.SaveChanges();

            ViewBag.gonderenHesap = gonderenHesap;
            ViewBag.alanHesap = alanHesap;
            ViewBag.aliciMus = aliciMus;
            ViewBag.miktar = havaleMiktar;


            return View();
        }

        public ActionResult AlanHesapKontrol(string hesapNo,string hesapEkNo)
        {
            dbBankEntities db = new dbBankEntities();
            Musteri musteri = (Musteri)Session["mus"];
            Musteri must = null;
            List<Musteri> mustList = db.Musteri.ToList();
            foreach (Musteri mus in mustList)
            {
                if (mus.hesapNo==hesapNo)
                {
                    must = mus;
                }
            }

            List<Hesap> hsp = null;
            if (must!=null)
            {
                hsp = db.Hesap.Where(h => h.musTCKN == must.tcKimlikNo && h.hesapEkNo == hesapEkNo && h.aktifmi == true).ToList();
            }

            if (must != null && hsp.Count > 0)
            {
                if (must.tcKimlikNo != musteri.tcKimlikNo)
                {
                    return Json(new { success = true, responseText = "var", musteri = must.tamAdi }, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(new { success = true, responseText = "yok"}, JsonRequestBehavior.AllowGet);
        }
    }
}