using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace online_shop.Controllers
{
    public class CategoriesController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();
        // GET: Category
        public ActionResult Index()
        {
            ViewBag.Categories = db.Categories;
            if(TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"];
            }
            return View();
        }
        public ActionResult Show(int id)
        {
            ViewBag.category = db.Categories.Find(id);
            return View();
        }

        // GET
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            return View(category);
        }
        [HttpPut]
        public ActionResult Edit(int id, Category requestCat)
        {
            try
            {
                Category cat = db.Categories.Find(id);

                if(TryUpdateModel(cat))
                {
                    cat.Title = requestCat.Title;
                    cat.Description = requestCat.Description;
                    db.SaveChanges();
                    TempData["message"] = "Categorie schimbata cu succes.";
                    return RedirectToAction("Index");
                }
                return View(requestCat);
            }catch(Exception e)
            {
                return View(requestCat);    
            }
        }

        public ActionResult New()
        {
            Category cat = new Category();
            ViewBag.modificare = "DA";
            return View(cat);
        }
        [HttpPost]
        public ActionResult New(Category newCat)
        {
            try
            {
                db.Categories.Add(newCat);
                db.SaveChanges();
                TempData["message"] = "Categorie noua adaugata cu succes.";
                return RedirectToAction("Index");
            }catch(Exception e)
            {
                ViewBag.modificare = "NU";
                return View(newCat);    
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Category cat = db.Categories.Find(id);
            db.Categories.Remove(cat);
            db.SaveChanges();
            TempData["message"] = "Categorie stearsa cu succes.";
            return RedirectToAction("Index");
        }
    }
}