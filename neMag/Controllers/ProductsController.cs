﻿using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class ProductsController : Controller
    {

        private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
        private int _perPage = 11; // how many products to show per page
        private const int PRICE_ASC = 1;
        private const int PRICE_DESC = 2;
        private const int RATING_ASC = 3;
        private const int RATING_DESC = 4;

        // everyone can see the available products
        public ActionResult Index()
        {
            //var products = (from prod in db.Products
            //                where prod.Accepted == true
            //                select prod).Include("Category").AsQueryable();
            int id;
            try
            {
                id = Request.Params.Get("id") != null ?
                     Int32.Parse(Request.Params.Get("id").Trim().ToString()) : 0;
            }
            catch (NullReferenceException e)
            {
                id = 0;
            }
            ViewBag.inorder = id;
            var products = (from prod in db.Products//daca nu pun AsQueryable nu ruleaza, no idea why
                            where prod.Accepted == true
                            select prod).Include("Category").AsQueryable();
            //id: 1= pret crescator;2 = pres descrescator; 3= rating crescator; 4 = rating descrescator
            //(comment dragos) am adaugat niste constante, ca nu vazusem comentariul de mai sus
            var x = false;
            if (id == PRICE_ASC)
            {
                products = (from prod in db.Products
                            where prod.Accepted == true
                            orderby (prod.Price - prod.Price * prod.Discount / 100)
                            select prod).Include("Category").AsQueryable();
                x = true;
            }
            else if (id == PRICE_DESC)
            {
                products = (from prod in db.Products
                            where prod.Accepted == true
                            orderby (prod.Price - prod.Price * prod.Discount / 100) descending
                            select prod).Include("Category").AsQueryable();
                x = true;
            }
            else if (id == RATING_ASC)
            {
                products = (from prod in db.Products
                            where prod.Accepted == true
                            orderby prod.Rating
                            select prod).Include("Category").AsQueryable();
                
                x = true;
            }
            else if (id == RATING_DESC)
            {
                products = (from prod in db.Products
                            where prod.Accepted == true
                            orderby prod.Rating descending
                            select prod).Include("Category").AsQueryable();
                x = true;
            }
            ViewBag.x = x;

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

        [Authorize (Roles ="Admin,Collaborator")]
        public ActionResult New()
        {
            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult New(Product product, HttpPostedFileBase[] uploadedPhotos)
        {
            product.Categ = GetAllCategories();
            product.UserId = User.Identity.GetUserId();
            product.User = db.Users.Find(product.UserId);
            try
            {
                if (ModelState.IsValid)
                {
                    product.Rating = 0;
                    product.Accepted = false;
                    product.Category = db.Categories.Find(product.CategoryId);
                    db.Products.Add(product);
                    db.SaveChanges();
                    PhotosController.UploadPhotos(uploadedPhotos, product.ProductId, true);

                    TempData["massage"] = "Produsul a fost adaugat";

                    return RedirectToAction("Index");
                }
                else
                {
                    product.Categ = GetAllCategories();
                    product.Price = 1;
                    TempData["massage"] = "Produsul nu a fost adaugat!";
                    return RedirectToAction("New");
                }
            }
            catch (Exception e)
            {
                product.Categ = GetAllCategories();
                product.Price = 2;
                TempData["massage"] = "Produsul nu a fost adaugat!";
                return RedirectToAction("New");
            }
        }

        public ActionResult Show(int id)
        {
            Product product = db.Products.Find(id);
            if (product == null)
            {
                TempData["message"] = "Bad id" + id;
                return RedirectToAction("Index");
            }
            ViewBag.Product = product;
            ViewBag.Category = product.Category;

            ViewBag.userId = User.Identity.GetUserId();

            SetAccessRights(product);

            return View(product);
        }

        [HttpDelete]
        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult Delete(int id)
        {
            Product product = db.Products.Find(id);
            if (User.IsInRole("Collaborator") &&
                product.UserId != User.Identity.GetUserId())
            {
                TempData["message"] = "Acces interzis";
                return RedirectToAction("Index");
            }

            // Delete the photos before deleting the product
            List<int> ids = new List<int>();
            foreach (var photo in product.Photos)
            {
                ids.Add(photo.PhotoId);
            }
            foreach (int photoId in ids)
            {
                // delete from server
                FileInfo fileInfo = new FileInfo(db.Photos.Find(photoId).Path);
                if (fileInfo.Exists)
                {
                    fileInfo.Delete();
                }

                db.Photos.Remove(db.Photos.Find(photoId));
            }





            db.Products.Remove(product);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult Edit(int id)
        {
            Product product = db.Products.Find(id);
            if(User.IsInRole("Collaborator") &&
                product.UserId != User.Identity.GetUserId())
            {
                TempData["message"] = "Acces interzis";
                return RedirectToAction("Index");
            }
            //ViewBag.OldPath = product.Photo;
            product.Categ = GetAllCategories();
            return View(product);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult Edit(int id, Product requestProduct, HttpPostedFileBase[] uploadedPhotos)
        {
            requestProduct.Categ = GetAllCategories();
           // if (photo == null)
           //     requestProduct.Photo = db.Products.Find(id).Photo;
            try
            {
               if (ModelState.IsValid)
               {
                    Product product = db.Products.Find(id);
                    if (User.IsInRole("Collaborator") &&
                        product.UserId != User.Identity.GetUserId())
                    {
                        TempData["message"] = "Acces interzis";
                        return RedirectToAction("Index");
                    }
                    if (TryUpdateModel(product))
                    {
                        product.ProductName = requestProduct.ProductName;
                        product.Description = requestProduct.Description;
                        product.CategoryId = requestProduct.CategoryId;
                        product.Category = db.Categories.Find(product.CategoryId);

                        /*
                        if (photo != null)
                        {
                            // var newPhotoPath = UploadPhoto(photo);
                            product.Photo = "";
                        }
                        else
                        {
                            product.Photo = requestProduct.Photo;
                        }
                        */
                        db.SaveChanges();
                        PhotosController.UploadPhotos(uploadedPhotos, product.ProductId, true);
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

        [Authorize(Roles = "Admin, Colaborator")]
        public ActionResult Requests()
        {

            var products = db.Products.Include("Category");
            ViewBag.Products = products;


            return View();
        }


        public ActionResult MyProducts(string id = "")
        {


            var products = from prod in db.Products
                           where prod.UserId == id
                           select prod;
            var anyPending = products.Where(p => p.Accepted == false).ToList().Any();
            var anyAccepted = products.Where(p => p.Accepted == true).ToList().Any();
            ViewBag.anyAccepted = anyAccepted;
            ViewBag.anyPending = anyPending;
            ViewBag.Products = products;

            return View();
        }

        [Authorize(Roles = "Admin")]
        public ActionResult Accept(int id)
        {
            Product product = db.Products.Find(id);
            product.Accepted = true;
            db.SaveChanges();
            return RedirectToAction("Requests");
        }

        [HttpDelete]
        [Authorize(Roles = "Admin")]
        public ActionResult DeleteRequest(int id)
        {
            Product product = db.Products.Find(id);
            db.Products.Remove(product);
            db.SaveChanges();


            return RedirectToAction("Requests");

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
                    Text = category.Title.ToString() // ToString might be redundant
                });
            }
            return selectList;
        }

        [NonAction]
        private void SetAccessRights(Product prod)
        {
            ViewBag.showButtons = false;
            ViewBag.isCollaborator = false;
            if (User.IsInRole("Admin")) // admin has full privileges on products
            {
                ViewBag.showButtons = true;
            }
            else if (User.IsInRole("Collaborator") && User.Identity.GetUserId() == prod.UserId) // collaborators only on theirs
            {
                ViewBag.showButtons = true;
                ViewBag.isCollaborator = true;
            }
        }
    }
}