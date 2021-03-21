using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class ProductsController : Controller
    {

        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        private int _perPage = 11; // cate produse intra pe o pagina
        
        // GET: Product
        public ActionResult Index()
        {
            var products = (from prod in db.Products//daca nu pun AsQueryable nu ruleaza, no idea why
                            select prod).Include("Category").AsQueryable();
            
            
            var totalItems = products.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
                offset = (currentPage - 1) * this._perPage;

            var categories = db.Categories;

            var prodsOnPage = products.ToList().Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            ViewBag.totalItems = totalItems;
            ViewBag.categories = categories;
            ViewBag.lastPage = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.Products = prodsOnPage;
            return View();
        }

        public ActionResult New()
        {
            Product produs = new Product();
            produs.Categ = GetAllCategories();
            return View(produs);
        }

        [HttpPost]
        public ActionResult New(Product produs, HttpPostedFileBase photo)
        {
            produs.Categ = GetAllCategories();
            try
            {
                if (ModelState.IsValid)
                {
                    produs.Rating = 0;
                    produs.Accepted = false;
                    var newPhotoPath = UploadPhoto(photo);
                    produs.Photo = newPhotoPath;
                    
                    db.Products.Add(produs);
                    db.SaveChanges();
                    TempData["massage"] = "Produsul a fost adaugat";
                    
                    return RedirectToAction("Index");
                }
                else
                {
                    produs.Categ = GetAllCategories();
                    produs.Price = 1;
                    return View("Index");
                }
            }
            catch (Exception e)
            {
                produs.Categ = GetAllCategories();
                produs.Price = 2;
                return View("Index");
            }


        }
        
        [NonAction]
        public string UploadPhoto(HttpPostedFileBase uploadedFile)
        {
            // TODO remove this, momentan e folositor doar pt debugging rapid, dar altfel n ar trebui sa fie un feature
            if (uploadedFile == null)
                return "";
            // Se preia numele fisierul
            string uploadedFileName = uploadedFile.FileName;
            string uploadedFileExtension = Path.GetExtension(uploadedFileName);

            // Se poate verifica daca extensia este intr-o lista dorita
            if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpg")
            {
                // Se stocheaza fisierul in folderul Files (folderul trebuie creat in proiect)

                // 1. Se seteaza calea folderului de upload
                string uploadFolderPath = Server.MapPath("~/Photos/");

                // 2. Se salveaza fisierul in acel folder
                uploadedFile.SaveAs(uploadFolderPath + uploadedFileName);

                // 3. Se adauga modelul in baza de date
                db.SaveChanges();

                // 4. returnez filepath-ul
                return "\\Photos\\" + uploadedFileName;
            }
            return "";
        }

        public ActionResult Show(int id)
        {
            Product produs = db.Products.Find(id);
            if (produs == null)
            {
                TempData["message"] = "Bad id" + id;
                return RedirectToAction("Index");
            }
            ViewBag.Product = produs;
            ViewBag.Category = produs.Category;

            SetAccessRights(produs);

            return View(produs);
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        public ActionResult Edit(int id)
        {
            Product produs = db.Products.Find(id);
            ViewBag.OldPath = produs.Photo;
            produs.Categ = GetAllCategories();
            return View(produs);
        }

        [HttpPut]
        public ActionResult Edit(int id, Product requestProduct, HttpPostedFileBase photo)
        {
            requestProduct.Categ = GetAllCategories();
            if (photo == null)
                requestProduct.Photo = db.Products.Find(id).Photo;
            try
            {
                if (ModelState.IsValid)
                {
                    Product produs = db.Products.Find(id);
                    if (TryUpdateModel(produs))
                    {
                        produs.ProductName = requestProduct.ProductName;
                        produs.Description = requestProduct.Description;
                        produs.CategoryId = requestProduct.CategoryId;

                        if (photo != null)
                        {
                            var newPhotoPath = UploadPhoto(photo);
                            produs.Photo = newPhotoPath;
                        }
                        else
                        {
                            produs.Photo = requestProduct.Photo;
                        }

                        db.SaveChanges();
                        TempData["message"] = "Produsul a fost modificat";
                        return RedirectToAction("Index");
                    }
                    else
                    {
                        return View(requestProduct);
                    }
                }
                else
                {
                    return View(requestProduct);
                }
            }
            catch (Exception e)
            {
                return View();
            }
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = from cat in db.Categories
                             select cat;

            foreach (var category in categories)
            {
                selectList.Add(new SelectListItem
                {
                    Value = category.CategoryId.ToString(),
                    Text = category.Title.ToString() // posibil sa nu fie necesar ToString
                });
            }
            return selectList;
        }

        [NonAction]
        private void SetAccessRights(Product prod)
        {   // momentan nu avem user roles, deci metoda asta inca nu face nimic
            ViewBag.afisareButoane = true;
        }
    }
}