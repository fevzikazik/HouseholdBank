using System;
using System.Collections.Generic;
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

        // GET: HGS/UyelikOlustur
        public async Task<ActionResult> UyelikOlustur()
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
        
        // POST: HGS/BakiyeGuncelle/41
        [HttpPost]
        public async Task<ActionResult> BakiyeGuncelle(int id, FormCollection collection)
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
                    
                    HGS hgs = null;


                    using (var resultt = await client.GetAsync("api/HGS/" + id))
                    {
                        if (resultt.IsSuccessStatusCode)
                        {
                            var value = resultt.Content.ReadAsStringAsync().Result;

                            hgs = JsonConvert.DeserializeObject<ResponseContent<HGS>>(value).Data.ToList().First();
                            
                        }
                    }
                    
                    hgs.bakiye += Convert.ToInt32(collection.Get("miktar"));

                    var serializedProduct = JsonConvert.SerializeObject(hgs);
                    var content = new StringContent(serializedProduct, Encoding.UTF8, "application/json");
                    var result = await client.PutAsync("api/HGS/" + id, content);
                    if (result.IsSuccessStatusCode)
                        return RedirectToAction("Index");

                    return RedirectToAction("BakiyeGuncelle", id);
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
    }
}