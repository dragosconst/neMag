using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    // TODO: Insert Authorize attributes.

    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();


        public ActionResult Index()
        {
            var posts = from post in db.Posts
                        select post;
            ViewBag.posts = posts;

            return View();
        }

        public ActionResult Edit(int id)
        {
            Post post = db.Posts.Find(id);
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Colaborator") || User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "You cannot edit someone else's post!";
                // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                return RedirectToAction("Index", "Posts"); // placeholder
            }
        }

        [HttpPut]
        //[Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Edit(int id, Post requestPost)
        {
            try
            {
                Post post = db.Posts.Find(id);
                if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Colaborator") || User.IsInRole("Admin"))
                {
                    if (ModelState.IsValid && TryUpdateModel(post))
                    {
                        post.Content = requestPost.Content;
                        post.Date = requestPost.Date;
                        db.SaveChanges();
                        // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                        return RedirectToAction("Index", "Posts"); // placeholder
                    }
                    return View(requestPost);
                }
                TempData["message"] = "You cannot edit someone else's post!";
                // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                return RedirectToAction("Index", "Posts"); // placeholder
            }
            catch (Exception e)
            {
                return View(requestPost);
            }
        }

        [HttpPost]
        //[Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult New(Post post)
        {
            post.Date = DateTime.Now;
            post.isReview = false; // placeholder
            post.UserId = User.Identity.GetUserId();

            try
            {
                if (ModelState.IsValid)
                {
                    db.Posts.Add(post);
                    db.SaveChanges();
                    TempData["message"] = "The post has been added.";
                }
                else
                    TempData["message"] = "Content is mandatory.";

                // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                return RedirectToAction("Index", "Posts"); // placeholder
            }
            catch (Exception e)
            {
                TempData["message"] = "The post was not added.";
                // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                return RedirectToAction("Index", "Posts"); // placeholder
            }
        }

        [HttpDelete]
        //[Authorize(Roles = "User,Colaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Find(id);
            // int ProductId = post.ProductId;

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Colaborator") || User.IsInRole("Admin"))
            {
                TempData["message"] = "The post has been deleted.";
                db.Posts.Remove(post);
                db.SaveChanges();
                // return RedirectToAction("Show", "Products", new { id = post.ProductId });
                return RedirectToAction("Index", "Posts"); // placeholder
            }

            TempData["message"] = "You cannot delete someone else's post!";
            // return RedirectToAction("Show", "Products", new { id = post.ProductId });
            return RedirectToAction("Index", "Posts"); // placeholder
        }
    }
}