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
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Collaborator") || User.IsInRole("Admin"))
            {
                return View(post);
            }
            else
            {
                TempData["message"] = "You cannot edit someone else's post!";
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
        }

        [HttpPut]
        //[Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult Edit(int id, Post requestPost)
        {
            try
            {
                Post post = db.Posts.Find(id);
                if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Collaborator") || User.IsInRole("Admin"))
                {
                    if (ModelState.IsValid && TryUpdateModel(post))
                    {
                        post.Content = requestPost.Content;
                        post.Date = requestPost.Date;
                        post.ProductId = requestPost.ProductId;
                        post.isReview = requestPost.isReview;
                        post.Rating = requestPost.Rating;
                        db.SaveChanges();
                        UpdateProductRating(post.ProductId);
                        return RedirectToAction("Show", "Products", new { id = post.ProductId });
                    }
                    return View(requestPost);
                }
                TempData["message"] = "You cannot edit someone else's post!";
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
            catch (Exception e)
            {
                return View(requestPost);
            }
        }

        [HttpPost]
        //[Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult New(Post post)
        {
            post.Date = DateTime.Now;
            post.isReview = true; // PLACEHOLDER: For now, all posts are reviews.
            post.UserId = User.Identity.GetUserId();

            try
            {
                if (ModelState.IsValid)
                {
                    db.Posts.Add(post);
                    db.SaveChanges();
                    UpdateProductRating(post.ProductId);
                    TempData["message"] = "The post has been added.";
                }
                else
                    TempData["message"] = "Content is mandatory.";

                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
            catch (Exception e)
            {
                TempData["message"] = "The post was not added.";
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
        }

        [HttpDelete]
        //[Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Find(id);
            // int ProductId = post.ProductId;

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Collaborator") || User.IsInRole("Admin"))
            {
                TempData["message"] = "The post has been deleted.";
                db.Posts.Remove(post);
                db.SaveChanges();
                UpdateProductRating(post.ProductId);
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }

            TempData["message"] = "You cannot delete someone else's post!";
            return RedirectToAction("Show", "Products", new { id = post.ProductId });
        }

        [NonAction]
        private double CalculateProductRating(int id)
        {
            double sum = 0.0;
            int n;
            var revs = from post in db.Posts
                       where post.ProductId == id && post.isReview == true
                       select post;

            n = revs.Count();
            foreach (var rev in revs)
                sum += rev.Rating;

            if (n == 0)
                return 0;
            else
                return Math.Round(sum / n, 2);
        }

        [NonAction]
        private void UpdateProductRating(int id)
        {
            Product product = db.Products.Find(id);
            product.Rating = CalculateProductRating(id);
            db.SaveChanges();
        }
    }
}