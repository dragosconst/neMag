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
            var users = from user in db.Users
                        orderby user.UserName
                        select user;
            ViewBag.UsersList = users;
            return View();
        }

        public ActionResult Show(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            var roles = (from userRole in user.Roles
                         join role in db.Roles on userRole.RoleId
                         equals role.Id
                         select role.Name).ToList();
            ViewBag.userRole = roles[0];

            var products = from prod in db.Products
                           where prod.UserId == user.Id
                           select prod;
            ViewBag.products = products;

            var orders = from ord in db.Orders
                         where ord.UserId == user.Id
                         select ord;
            ViewBag.orders = orders;

            var posts = from post in db.Posts
                        where post.UserId == user.Id
                        select post;
            ViewBag.posts = posts;

            var fav = from users in db.UserProducts
                      join prod in db.Products on users.ProductId equals prod.ProductId
                      where users.UserId == user.Id
                      select prod;
            ViewBag.fav = fav;

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
    }
}