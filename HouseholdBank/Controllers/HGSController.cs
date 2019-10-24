using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Globalization;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using HouseholdBank.Models;
using Newtonsoft.Json;

namespace HouseholdBank.Controllers
{
    public class HGSController : Controller
    {
        string Baseurl = "https://householdwebapi.azurewebsites.net/";
        //string Baseurl = "http://localhost:57619/";

        // GET: HGS
        public async Task<ActionResult> Index()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();
            Musteri mus = (Musteri)Session["mus"];

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

                    IList<HGS> hgsListesi = null;
                    using (var result = await client.GetAsync("api/HGS"))
                    {
                        if (result.IsSuccessStatusCode)
                        {
                            var value = result.Content.ReadAsStringAsync().Result;

                            hgsListesi = JsonConvert.DeserializeObject<ResponseContent<HGS>>(value).Data.ToList();
                            
                        }
                    }

                    ViewBag.hgsListesi = hgsListesi;
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }

        // POST: HGS/UyelikOlustur
        [HttpPost]
        public async Task<ActionResult> UyelikOlustur(FormCollection collection)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");


            dbBankEntities db = new dbBankEntities();
            Musteri mus = (Musteri)Session["mus"];

            if (!ModelState.IsValid)
            {
                return View();
            }
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    HGS hgs = new HGS();
                    hgs.musTCKN = mus.tcKimlikNo;
                    hgs.bakiye = 0;
                    hgs.aktifmi = true;
                    hgs.plaka = collection.Get("plaka");

                    var serializedProduct = JsonConvert.SerializeObject(hgs);
                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("api/HGS", content);
                    if (result.IsSuccessStatusCode)
                        return RedirectToAction("Index");


                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }
        
        // GET: HGS/IptalEt/41
        public async Task<ActionResult> IptalEt(int id)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    var result = await client.DeleteAsync("api/HGS/" + id);
                    if (result.IsSuccessStatusCode)
                        return RedirectToAction("Index");
                    return RedirectToAction("Index");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }

        [HttpPost]
        public ActionResult BakiyeHesap(int id)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            ViewBag.hgsID = id;
            return View();
        }

        [HttpPost]
        public ActionResult BakiyeYukle(FormCollection fc)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            int secilenHGSID = Convert.ToInt32(fc.Get("secilenHgsID"));
            string secilenHesap = fc.Get("secilenHesap");
            ViewBag.secilenHGSID = secilenHGSID;
            ViewBag.secilenHesap = secilenHesap;

            return View();
        }

        // POST: HGS/BakiyeGuncelle
        [HttpPost]
        public async Task<ActionResult> BakiyeGuncelle(FormCollection collection)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    dbBankEntities db = new dbBankEntities();
                    Musteri mus = (Musteri)Session["mus"];

                    decimal yuklenecekMiktar = Convert.ToDecimal(collection.Get("miktar"), new CultureInfo("tr-TR"));
                    string secilenHesapEkNo = collection.Get("secilenHesap");
                    Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == secilenHesapEkNo && h.aktifmi == true).Single();

                    secilenHesap.bakiye -= yuklenecekMiktar;
                    db.Hesap.Attach(secilenHesap);
                    db.Entry(secilenHesap).State = EntityState.Modified;

                    HGS hgs = null;

                    int secilenHGSID = Convert.ToInt32(collection.Get("secilenHGSID"));

                    using (var resultt = await client.GetAsync("api/HGS/" + secilenHGSID))
                    {
                        if (resultt.IsSuccessStatusCode)
                        {
                            var value = resultt.Content.ReadAsStringAsync().Result;

                            hgs = JsonConvert.DeserializeObject<ResponseContent<HGS>>(value).Data.ToList().First();

                        }
                    }

                    Islem isl = new Islem();
                    isl.hesapNo = mus.hesapNo;
                    isl.hesapEkNo = secilenHesap.hesapEkNo;
                    isl.islemTipi = "HGS Yüklemesi";
                    isl.tarih = DateTime.Now;
                    isl.aciklama = yuklenecekMiktar + " TL " + hgs.plaka + " Plakalı Araç İçin HGS Yüklemesi Yapildi!";
                    isl.platform = "web";
                    db.Islem.Add(isl);
                    db.SaveChanges();

                    hgs.bakiye += yuklenecekMiktar;

                    var serializedProduct = JsonConvert.SerializeObject(hgs);
                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                    var result = await client.PutAsync("api/HGS/" + secilenHGSID, content);
                    if (result.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    return RedirectToAction("BakiyeGuncelle", secilenHGSID);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }


        [HttpPost]
        public ActionResult BakiyeBosaltHesap(int id)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            ViewBag.hgsID = id;
            return View();
        }
        
        // POST: HGS/BakiyeAktar
        [HttpPost]
        public async Task<ActionResult> BakiyeBosalt(FormCollection collection)
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            if (!ModelState.IsValid)
            {
                return RedirectToAction("Index");
            }
            try
            {
                using (var client = new HttpClient())
                {
                    client.BaseAddress = new Uri(Baseurl);
                    client.DefaultRequestHeaders.Accept.Clear();
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    dbBankEntities db = new dbBankEntities();
                    Musteri mus = (Musteri)Session["mus"];

                    string secilenHesapEkNo = collection.Get("secilenHesap");
                    Hesap secilenHesap = db.Hesap.Where(h => h.musTCKN == mus.tcKimlikNo && h.hesapEkNo == secilenHesapEkNo && h.aktifmi == true).Single();
                    

                    HGS hgs = null;

                    int secilenHGSID = Convert.ToInt32(collection.Get("secilenHGSID"));

                    using (var resultt = await client.GetAsync("api/HGS/" + secilenHGSID))
                    {
                        if (resultt.IsSuccessStatusCode)
                        {
                            var value = resultt.Content.ReadAsStringAsync().Result;

                            hgs = JsonConvert.DeserializeObject<ResponseContent<HGS>>(value).Data.ToList().First();

                        }
                    }

                    decimal bosaltilacakMiktar = Convert.ToDecimal(hgs.bakiye, new CultureInfo("tr-TR"));
                    secilenHesap.bakiye += bosaltilacakMiktar;
                    db.Hesap.Attach(secilenHesap);
                    db.Entry(secilenHesap).State = EntityState.Modified;

                    Islem isl = new Islem();
                    isl.hesapNo = mus.hesapNo;
                    isl.hesapEkNo = secilenHesap.hesapEkNo;
                    isl.islemTipi = "HGS İadesi";
                    isl.tarih = DateTime.Now;
                    isl.aciklama = bosaltilacakMiktar + " TL " + hgs.plaka + " Plakalı Araç İçin HGS İadesi Yapildi!";
                    isl.platform = "web";
                    db.Islem.Add(isl);
                    db.SaveChanges();

                    hgs.bakiye -= bosaltilacakMiktar;

                    var serializedProduct = JsonConvert.SerializeObject(hgs);
                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                    var result = await client.PutAsync("api/HGS/" + secilenHGSID, content);
                    if (result.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    return RedirectToAction("BakiyeAktar", secilenHGSID);
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }

    }
}