using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Security;
using HouseholdBank.Models;

namespace HouseholdBank.Controllers
{
    public class LoginController : Controller
    {
        // GET: Login
        [HttpGet]
        public ActionResult Index()
        {
            if (Session["mus"] == null)
            {
                return View();
            }
            return RedirectToAction("Index", "Profil");
        }

        // GET: Login
        [HttpPost]
        public ActionResult GirisKontrol(Musteri m)
        {
            dbBankEntities db = new dbBankEntities();
            var _mus = db.Musteri.Where(s => s.tcKimlikNo == m.tcKimlikNo);
            if (_mus.Any())
            {
                if (_mus.Where(s => s.sifre == m.sifre).Any())
                {
                    Session["mus"] = _mus.SingleOrDefault();

                    return RedirectToAction("Index", "Profil");
                }
                else
                {
                    return RedirectToAction("Index", "Login");
                }
            }
            else
            {
                return RedirectToAction("Index", "Login");
            }
        }


        // GET: Signup
        public ActionResult Signup()
        {
            if (Session["mus"] == null)
            {
                return View();
            }
            return RedirectToAction("Index", "Profil");
        }


        public ActionResult TCKNKontrol(string input)
        {
            dbBankEntities db = new dbBankEntities();
            List<Musteri> must = db.Musteri.Where(mus => mus.tcKimlikNo == input).ToList();
            if (must.Count > 0)
            {
                return Json(new { success = true, responseText = "var" }, JsonRequestBehavior.AllowGet);
            }

            return Json(new { success = true, responseText = "yok" }, JsonRequestBehavior.AllowGet);
        }

        [HttpPost]
        public ActionResult KayitKontrol(Musteri m)
        {

            dbBankEntities db = new dbBankEntities();
            
            Random rnd = new Random();
            bool essiz = false;
            double hesapNo;
            do
            {
                hesapNo = rnd.Next(100000000, 999999999);
                string hspNo = Convert.ToString(hesapNo);
                List<Musteri> hesap = db.Musteri.Where(mus => mus.hesapNo == hspNo).ToList();
                if (hesap.Count==0)
                {
                    essiz = true;
                }
            } while (!essiz);

            Musteri musteri = new Musteri()
            {
                tcKimlikNo = m.tcKimlikNo,
                tamAdi = m.tamAdi,
                sifre = m.sifre,
                adres = m.adres,
                telefon = m.telefon,
                dogumTarihi = m.dogumTarihi,
                ePosta = m.ePosta,
                hesapNo = hesapNo.ToString()
            };

            Hesap yenihesap = new Hesap();
            yenihesap.aktifmi = true;
            yenihesap.bakiye = 0;
            yenihesap.acilisTarihi = DateTime.Now.Date;
            yenihesap.hesapEkNo = "5001";
            yenihesap.musTCKN = m.tcKimlikNo;

            db.Musteri.Add(musteri);
            db.Hesap.Add(yenihesap);
            db.SaveChanges();

            return RedirectToAction("Index", "Login");
        }


        public ActionResult Cikis()
        {
            FormsAuthentication.SignOut();
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }
    }
}