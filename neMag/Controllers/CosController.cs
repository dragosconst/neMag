using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class CosController : Controller
    {
        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        // GET: Cos
        /*
        public ActionResult Index()
        {
            var uid = User.Identity.GetUserId();
            // UpdateCosValue(); // trebuie implementata metoda
            // fiecare user trebuie sa aiba un order cu status cos
            // var cos = getCos(); // trebuie implementata metoda
            var cosElems = from product in db.Products
                           join orderCon in db.OrderContents on product.ProductId equals orderCon.ProductId
                           where orderCon.OrderId == cos.OrderId
                           select product; // selectez toate produsele care se gasesc in cos
            var orderCons = from orderCon in db.OrderContents
                            where orderCon.OrderId == cos.OrderId
                            select orderCon;
            ViewBag.CosElems = cosElems;
            ViewBag.OCs = orderCons;
            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View(cos);
        }
        */


    }
}