﻿using Microsoft.AspNet.Identity;
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
        private int _perPage = 11; // how many products to show per page

        // everyone can see the available products
        public ActionResult Index()
        {
            var products = (from prod in db.Products
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

        [Authorize (Roles ="Admin,Collaborator")]
        public ActionResult New()
        {
            Product product = new Product();
            product.Categ = GetAllCategories();
            return View(product);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult New(Product product, HttpPostedFileBase photo)
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
                    var newPhotoPath = UploadPhoto(photo);
                    product.Photo = newPhotoPath;
                    product.Category = db.Categories.Find(product.CategoryId);

                    db.Products.Add(product);
                    db.SaveChanges();
                    TempData["massage"] = "Produsul a fost adaugat";

                    return RedirectToAction("Index");
                }
                else
                {
                    product.Categ = GetAllCategories();
                    product.Price = 1;
                    return View("Index");
                }
            }
            catch (Exception e)
            {
                product.Categ = GetAllCategories();
                product.Price = 2;
                return View("Index");
            }


        }

        [NonAction]
        public string UploadPhoto(HttpPostedFileBase uploadedFile)
        {
            // do we want to accept products with no photos? if not, this if could be removed
            if (uploadedFile == null)
                return "";
            // Get file name
            string uploadedFileName = uploadedFile.FileName;
            string uploadedFileExtension = Path.GetExtension(uploadedFileName);

            // Check some extensions
            if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpg")
            {
                // With the current implementation, the Photos folder must be manually created
                // TODO: fix this, maybe, or improve on it

                // 1. Map the upload file path
                string uploadFolderPath = Server.MapPath("~/Photos/");

                // 2. Save file in folder path
                uploadedFile.SaveAs(uploadFolderPath + uploadedFileName);

                // 3. Save database state
                db.SaveChanges();

                // 4. return filepath
                return "\\Photos\\" + uploadedFileName;
            }
            return "";
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
            ViewBag.OldPath = product.Photo;
            product.Categ = GetAllCategories();
            return View(product);
        }

        [HttpPut]
        [Authorize(Roles = "Admin,Collaborator")]
        public ActionResult Edit(int id, Product requestProduct, HttpPostedFileBase photo)
        {
            requestProduct.Categ = GetAllCategories();
            if (photo == null)
                requestProduct.Photo = db.Products.Find(id).Photo;
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

                        if (photo != null)
                        {
                            var newPhotoPath = UploadPhoto(photo);
                            product.Photo = newPhotoPath;
                        }
                        else
                        {
                            product.Photo = requestProduct.Photo;
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