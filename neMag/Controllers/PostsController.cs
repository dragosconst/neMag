﻿using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
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
        [Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult Edit(int id, Post requestPost, HttpPostedFileBase[] uploadedPhotos)
        {
            try
            {
                Post post = db.Posts.Find(id);
                if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
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
                        PhotosController.UploadPhotos(uploadedPhotos, post.PostId, false);
                        return RedirectToAction("Show", "Products", new { id = post.ProductId });
                    }
                    System.Diagnostics.Debug.WriteLine("Ceva.");
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
        [Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult New(Post post, HttpPostedFileBase[] uploadedPhotos)
        {
            post.Date = DateTime.Now;
            // post.isReview = true; // PLACEHOLDER: For now, all posts are reviews.
            post.UserId = User.Identity.GetUserId();

            try
            {
                // Make sure that a user cannot make multiple reviews on the same product
                if ((post.isReview &&
                    !(from p in db.Posts where p.UserId == post.UserId && p.ProductId == post.ProductId && p.isReview select p).Any())
                    || !post.isReview)
                {

                    if (ModelState.IsValid)
                    {
                        db.Posts.Add(post);
                        db.SaveChanges();
                        UpdateProductRating(post.ProductId);
                        PhotosController.UploadPhotos(uploadedPhotos, post.PostId, false);
                        TempData["message"] = "Mesajul a fost postat.";
                    }
                    else
                        TempData["message"] = "Continutul este obligatoriu.";

                }
                else
                    TempData["message"] = "Nu puteti lasa mai mult de o recenzie.";
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
            catch (Exception e)
            {
                TempData["message"] = "The post was not added.";
                return RedirectToAction("Show", "Products", new { id = post.ProductId });
            }
        }

        [HttpDelete]
        [Authorize(Roles = "User,Collaborator,Admin")]
        public ActionResult Delete(int id)
        {
            Post post = db.Posts.Find(id);
            // int ProductId = post.ProductId;

            if (post.UserId == User.Identity.GetUserId() || User.IsInRole("Admin"))
            {
                TempData["message"] = "The post has been deleted.";

                // Delete the photos before the post.
                List<int> ids = new List<int>();
                foreach (var photo in post.Photos)
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
                       where post.ProductId == id && post.isReview
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