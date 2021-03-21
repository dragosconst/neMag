﻿using neMag.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class PostsController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public ActionResult Edit(int id)
        {
            Post post = db.Posts.Find(id);
            return View(post);
        }

        [HttpPut]
        public ActionResult Edit(int id, Post requestPost)
        {
            try
            {
                Post post = db.Posts.Find(id);
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
            catch (Exception e)
            {
                return View(requestPost);
            }
        }

        [HttpPost]
        public ActionResult New(Post post)
        {
            post.Date = DateTime.Now;
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
        public ActionResult Delete(int id1, int id2)
        {
            Post post = db.Posts.Find(id1, id2);
            // int TopicId = post.ProductId;

            TempData["message"] = "The post has been deleted.";
            db.Posts.Remove(post);
            db.SaveChanges();
            // return RedirectToAction("Show", "Products", new { id = post.ProductId });
            return RedirectToAction("Index", "Posts"); // placeholder
        }
    }
}