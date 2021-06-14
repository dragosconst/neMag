using neMag.Models;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class UsersController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        [Authorize(Roles = "Admin")]
        public ActionResult Index()
        {
            var users = db.Users.OrderBy(u => u.UserName);
            ViewBag.UsersList = users;
            return View();
        }

        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = user.Roles.Join(db.Roles, ur => ur.RoleId, r => r.Id, (ur, r) => r.Name).ToList();
            ViewBag.userRole = roles[0];

            var products = db.Products.Where(p => p.UserId == user.Id);
            ViewBag.products = products;

            var orders = db.Orders.Where(o => o.UserId == user.Id);
            ViewBag.orders = orders;

            var posts = db.Posts.Where(p => p.UserId == user.Id);
            ViewBag.posts = posts;

            var fav = from users in db.UserProducts
                      join prod in db.Products on users.ProductId equals prod.ProductId
                      where users.UserId == user.Id
                      select prod;
            ViewBag.fav = fav;


            var prodNr = db.Products.Where(p => p.UserId == user.Id).Count();
            ViewBag.prodNr = prodNr;

            if(prodNr != 0)
            {
                var maxPrice = db.Products.Where(p => p.UserId == user.Id).Max(p => p.Price);
                ViewBag.maxPrice = maxPrice;

                var minPrice = db.Products.Where(p => p.UserId == user.Id).Min(p => p.Price);
                ViewBag.minPrice = minPrice;

                var avgPrice = db.Products.Where(p => p.UserId == user.Id).Average(p => p.Price);
                ViewBag.avgPrice = avgPrice;
            }

            var ReviewNr = db.Posts.Where(p => p.UserId == user.Id && p.isReview == true).Count();
            ViewBag.ReviewNr = ReviewNr;
            if (ReviewNr != 0)
            {
                var avgReview = db.Posts.Where(p => p.UserId == user.Id && p.isReview == true).Max(p => p.Rating);
                ViewBag.avgReview = avgReview;
            }

            return View(user);
        }

        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            user.AllRoles = GetAllRoles();
            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            try
            {
                ApplicationDbContext context = new ApplicationDbContext();
                var roleManager = new RoleManager<IdentityRole>(new RoleStore<IdentityRole>(context));
                var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));

                if (TryUpdateModel(user))
                {
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;

                    var selectedRole = db.Roles.Find(HttpContext.Request.Params.Get("newRole"));
                    UserManager.AddToRole(id, selectedRole.Name);

                    db.SaveChanges();
                }
                return RedirectToAction("Show", new { id = id });
            }
            catch (Exception e)
            {
                Response.Write(e.Message);
                newData.Id = id;
                return View(newData);
            }
        }
        
        [HttpDelete]
        public ActionResult Delete(string id)
        {

            ApplicationDbContext context = new ApplicationDbContext();
            var UserManager = new UserManager<ApplicationUser>(new UserStore<ApplicationUser>(context));
            var user = UserManager.Users.FirstOrDefault(u => u.Id == id);

            var posts = db.Posts.Where(p => p.UserId == id);
            foreach (var post in posts)
            {
                db.Posts.Remove(post);
            }

            var products = db.Products.Where(p => p.UserId == id);
            foreach (var product in products)
            {
                db.Products.Remove(product);
            }

            db.SaveChanges();
            UserManager.Delete(user);
            return RedirectToAction("Index");
        }

        [NonAction]
        public IEnumerable<SelectListItem> GetAllRoles()
        {
            var selectList = new List<SelectListItem>();
            

            var roleStore = new RoleStore<IdentityRole>(db);
            var roleMngr = new RoleManager<IdentityRole>(roleStore);

            var roles = roleMngr.Roles.ToList();

            foreach (var r in roles)
            {
                selectList.Add(new SelectListItem
                {
                    Value = r.Id.ToString(),
                    Text = r.Name.ToString() // ToString might be redundant
                });
            }
            return selectList;
        }

        [HttpPut]
        public ActionResult AddToFav(int id)
        {
            if (!User.Identity.IsAuthenticated)
            {
                return RedirectToAction("Login", "Account");
            }
            
            UserProducts userProd = new UserProducts();
            userProd.ProductId = id;
            userProd.UserId = User.Identity.GetUserId();
            try
            {
                db.UserProducts.Add(userProd);
                db.SaveChanges();
               
                return RedirectToAction("Show", "Products", new { id = id });
            }
            catch (Exception e)
            {
                return RedirectToAction("Show", "Products", new { id = id });
            }
        }

        [HttpPut]
        public ActionResult RemoveFromFav(int id)
        {
            UserProducts userProd = db.UserProducts.Find(User.Identity.GetUserId(), id);
            db.UserProducts.Remove(userProd);
            db.SaveChanges();
            return RedirectToAction("Show", "Products", new { id = id });
        }
    }
}