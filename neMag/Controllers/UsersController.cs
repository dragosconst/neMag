﻿using neMag.Models;
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

        //[Authorize(Roles = "Admin")]
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
            return View(user);
        }

        public ActionResult Edit(string id)
        {
            ApplicationUser user = db.Users.Find(id);
            return View(user);
        }

        [HttpPut]
        public ActionResult Edit(string id, ApplicationUser newData)
        {
            ApplicationUser user = db.Users.Find(id);
            try
            {
                if (TryUpdateModel(user))
                {
                    user.FirstName = newData.FirstName;
                    user.LastName = newData.LastName;
                    user.Email = newData.Email;
                    user.PhoneNumber = newData.PhoneNumber;

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

        //[Authorize(Roles = "Admin")]
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
    }
}