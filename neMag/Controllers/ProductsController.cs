using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
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
            var products = db.Products.Where(p => p.Accepted == true).Include("Category").AsQueryable();
            var search = "";
            if (Request.Params.Get("search") != null)
            {
                search = Request.Params.Get("search").Trim(); // maybe implement some smarter search? right now I only look for substrings
                if (search != "")
                {
                    products = products.Where(p => p.ProductName.ToUpper().Contains(search.ToUpper()))
                                      .AsQueryable();
                }
            }

            var removeFilter = false;
            if (id == PRICE_ASC)
            {
                products = products.OrderBy(p => p.Price - p.Price * p.Discount / 100).Include("Category").AsQueryable();
                removeFilter = true;
            }
            else if (id == PRICE_DESC)
            {
                products = products.OrderByDescending(p => p.Price - p.Price * p.Discount / 100).Include("Category").AsQueryable();
                removeFilter = true;
            }
            else if (id == RATING_ASC)
            {
                products = products.OrderBy(p => p.Rating).Include("Category").AsQueryable();
                removeFilter = true;
            }
            else if (id == RATING_DESC)
            {
                products = products.OrderByDescending(p => p.Rating).Include("Category").AsQueryable();
                removeFilter = true;
            }

            var totalItems = products.Count();
            var currentPage = Convert.ToInt32(Request.Params.Get("page"));

            var offset = 0;

            if (!currentPage.Equals(0))
                offset = (currentPage - 1) * this._perPage;

            var categories = db.Categories;
            var crrCateg = 0;
            if (Request.Params.Get("category") != null && Request.Params.Get("category").Trim().ToString() != "")
            {
                System.Diagnostics.Debug.WriteLine(Request.Params.Get("category").Trim().ToString());
                crrCateg = Convert.ToInt32(Request.Params.Get("category").Trim().ToString());

                if (crrCateg != 0)
                    products = products.Where(p => p.CategoryId.Equals(crrCateg));
            }
            ViewBag.cat = crrCateg;



            var prodsOnPage = products.ToList().Skip(offset).Take(this._perPage);

            if (TempData.ContainsKey("message"))
            {
                ViewBag.Message = TempData["message"].ToString();
            }
            ViewBag.totalItems   = totalItems;
            ViewBag.search       = search;
            ViewBag.removeFilter = removeFilter;
            ViewBag.categories   = categories;
            ViewBag.lastPage     = Math.Ceiling((float)totalItems / (float)this._perPage);
            ViewBag.currentPage  = currentPage;
            ViewBag.Products     = prodsOnPage;
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
            Thread.CurrentThread.CurrentCulture = CultureInfo.GetCultureInfo("us"); // placeholder
            Thread.CurrentThread.CurrentUICulture = CultureInfo.GetCultureInfo("us");
            if (product == null)
            {
                TempData["message"] = "Bad id" + id;
                return RedirectToAction("Index");
            }
            ViewBag.Product = product;
            ViewBag.Category = product.Category;

            ViewBag.userId = User.Identity.GetUserId();
            ViewBag.reviews = product.Posts.Where(p => p.isReview).ToList();
            ViewBag.posts = product.Posts.Where(p => !p.isReview).ToList(); // posts from Q&A section
            var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(db));
            ViewBag.collaboratorRoleId = roleManager.Roles.Where(r => r.Name == "Collaborator").First().Id;
            ViewBag.adminRoleId = roleManager.Roles.Where(r => r.Name == "Admin").First().Id;
            ViewBag.postsPerPage = 4;
            ViewBag.pagesReviews = Math.Ceiling((double) ViewBag.reviews.Count / ViewBag.postsPerPage);
            ViewBag.pagesQA = Math.Ceiling((double) ViewBag.posts.Count / ViewBag.postsPerPage);

            if (TempData.ContainsKey("message"))
                ViewBag.Message = TempData["message"].ToString();

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

        [Authorize(Roles = "Admin")]
        public ActionResult Requests()
        {

            var products = db.Products.Include("Category");
            ViewBag.Products = products;


            return View();
        }


        public ActionResult MyProducts(string id = "")
        {


            var products = db.Products.Where(p => p.UserId == id);
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


            return RedirectToAction("Requests");

        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllCategories()
        {
            var selectList = new List<SelectListItem>();

            var categories = db.Categories;

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