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
using System.Web.Script.Serialization;
using HouseholdBank.Models;
using Newtonsoft.Json;

namespace HouseholdBank.Controllers
{
    public class KrediController : Controller
    {
        string Baseurl = "https://householdcreditscoring.azurewebsites.net/";
        //string Baseurl = "http://localhost:57619/";

        // GET: Kredi
        public ActionResult Index()
        {
            if (Session["mus"] == null)
                return RedirectToAction("Index", "Login");

            dbBankEntities db = new dbBankEntities();
            Musteri mus = (Musteri)Session["mus"];

            return View();
        }

        // POST: Kredi/KrediSonuc
        [HttpPost]
        public async Task<ActionResult> KrediSonuc(FormCollection fc)
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
                    client.DefaultRequestHeaders.Accept.Add(
                        new MediaTypeWithQualityHeaderValue("application/json"));

                    Kredi kredi = new Kredi();
                    kredi.krediMiktari = Convert.ToInt32(fc.Get("krediMiktari"));
                    kredi.yas = Convert.ToInt32(fc.Get("yas"));
                    kredi.aldigi_kredi_sayi = Convert.ToInt32(fc.Get("aldigi_kredi_sayi"));
                    kredi.evDurumu = fc.Get("evDurumu");
                    kredi.telefonDurumu = fc.Get("telefonDurumu");



                    var serializedProduct = JsonConvert.SerializeObject(kredi);
                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                    var result = await client.PostAsync("test", content);


                    IList<KrediResponse> sonuclar = null;
                    if (result.IsSuccessStatusCode)
                    {
                        var value = result.Content.ReadAsStringAsync().Result;

                        sonuclar = JsonConvert.DeserializeObject<IList<KrediResponse>>(value).ToList();
                    }
                    result.Dispose();
                    ViewBag.sonuclar = sonuclar;
                    return View();
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Hata Oluştu" + ex.Message);
            }
        }
    }
}