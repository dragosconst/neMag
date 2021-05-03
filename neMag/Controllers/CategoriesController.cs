using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace online_shop.Controllers
{
    [Authorize(Roles = "Admin")]
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
            Category category = db.Categories.Find(id);
            return View(category);
        }

        // GET
        public ActionResult Edit(int id)
        {
            Category category = db.Categories.Find(id);
            ViewBag.categories = GetAllCategories(category);
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
                    cat.ParentId = requestCat.ParentId;
                    cat.Description = requestCat.Description;
                    cat.ParentId = requestCat.ParentId;
                    if (requestCat.ParentId > 0)
                    {
                        cat.Parent = db.Categories.Find(requestCat.ParentId);
                    }
                    db.SaveChanges();
                    TempData["message"] = "Categorie schimbată cu succes.";
                    return RedirectToAction("Index");
                }
                ViewBag.categories = GetAllCategories(null);
                return View(requestCat);
            }catch(Exception e)
            {
                ViewBag.categories = GetAllCategories(null);
                return View(requestCat);    
            }
        }

        public ActionResult New()
        {
            Category cat = new Category();
            ViewBag.categories = GetAllCategories(null);
            return View(cat);
        }
        [HttpPost]
        public ActionResult New(Category newCat)
        {
            try
            {
                if (newCat.ParentId > 0)
                {
                    newCat.Parent = db.Categories.Find(newCat.ParentId);
                }
                db.Categories.Add(newCat);
                db.SaveChanges();
                TempData["message"] = "Categorie nouă adaugată cu succes.";
                return RedirectToAction("Index");
            }catch(Exception e)
            {
                ViewBag.categories = GetAllCategories(null);
                return View(newCat);    
            }
        }

        [HttpDelete]
        public ActionResult Delete(int id)
        {
            Category cat = db.Categories.Find(id);
            /*
             * Categories can only be deleted if there are no items listed under them and if they
             * don't have any subcategories. Possible improvement: if all subcategories are empty,
             * delete them all at once when their parent is removed.
             */
            bool hasSubcateg = db.Categories.Where(c => c.ParentId == cat.CategoryId).Any();
            if(hasSubcateg)
            {
                TempData["message"] = "Nu se poate șterge o categorie care are alte subcategorii în ierarhie.";
                return RedirectToAction("Index");
            }
            bool hasProducts = db.Products.Where(p => p.CategoryId == cat.CategoryId).Any();
            if(hasProducts)
            {
                TempData["message"] = "Nu se poate șterge o categorie care are produse în baza de date.";
                return RedirectToAction("Index");
            }
            db.Categories.Remove(cat);
            db.SaveChanges();
            TempData["message"] = "Categorie ștearsa cu succes.";
            return RedirectToAction("Index");
        }

        [NonAction]
        private IEnumerable<SelectListItem> GetAllCategories(Category me)
        {
            var selectList = new List<SelectListItem>();
            selectList.Add(new SelectListItem
            {
                Value = "-1",
                Text = "nicio categorie părinte"
            });
            foreach (Category cat in (db.Categories).ToList())
            {
                if (me != null && cat != me && !IsParent(cat.CategoryId, me.CategoryId))
                {
                    selectList.Add(new SelectListItem
                    {
                        Value = cat.CategoryId.ToString(),
                        Text = cat.Title.ToString() // ToString might be redundant
                    });
                }
            }
            return selectList;
        }


        /*
         * Might be a bit overkill, but checks for all the parents of the current category.
         * If we check just for the immediate parent, we might end up with more complex
         * circular dependencies (take for instance the hierarchy c1->c2->c3, without a more
         * thorough checking mechanism we might think having c3 as the parent of c1 would be
         * alright).
         * **/
        [NonAction]
        private bool IsParent(int childId, int parId)
        {
            /*
             * childId is 0 when a new category is added without specifying any parent
             * and it's -1 when specifically selecting no parent option in the dropdown.
             * Maybe setting it to 0 in both cases would be a better idea? I used -1 since
             * I'm not sure how the default asp.net c# assignment works when you don't select
             * anything in the dropdown.
             * **/
            while(childId > 0)
            {
                Category curr = db.Categories.Find(childId);
                if (parId == curr.ParentId)
                    return true;
                childId = curr.ParentId;
            }
            return false;
        }
    }
}