using Microsoft.AspNet.Identity;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;

namespace neMag.Controllers
{
    public class PhotosController : Controller
    {
        private ApplicationDbContext db = new ApplicationDbContext();

        public static void UploadPhotos(HttpPostedFileBase[] uploadedFiles, int Id, bool forProduct)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            foreach (var uploadedFile in uploadedFiles)
            {
                if (uploadedFile == null)
                    continue;

                string uploadedFileName = uploadedFile.FileName;
                string uploadedFileExtension = Path.GetExtension(uploadedFileName);
                
                if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpg")
                {
                    Photo file = new Photo();
                    file.Extension = uploadedFileExtension;
                    file.Name = uploadedFileName;
                    file.UserId = System.Web.HttpContext.Current.User.Identity.GetUserId();

                    string path;
                    if (forProduct)
                    {
                        path = "~/Photos/Products/" + Id + "/";
                        file.ProductId = Id;
                        file.PostId = null;
                    }
                    else
                    {
                        path = "~/Photos/Posts/" + Id + "/";   
                        file.PostId = Id;
                        file.ProductId = null;
                    }
                    file.Path = Path.Combine(path.Remove(0, 1), uploadedFileName);
                    if (!System.IO.Directory.Exists(HostingEnvironment.MapPath(path)))
                        System.IO.Directory.CreateDirectory(HostingEnvironment.MapPath(path));

                    string uploadFolderPath = HostingEnvironment.MapPath(path);
                    uploadedFile.SaveAs(Path.Combine(uploadFolderPath, uploadedFileName));

                    
                    db.Photos.Add(file);
                }
            }

            db.SaveChanges();
        }

        //[HttpDelete]
        [Authorize(Roles = "Admin,Collaborator,User")]
        public ActionResult Delete(int id)
        {
            Photo photo = db.Photos.Find(id);
            if (User.IsInRole("Admin") || photo.UserId == User.Identity.GetUserId())
            {
                var ProductId = photo.ProductId;
                var PostId = photo.PostId;

                // delete from server
                FileInfo fileInfo = new FileInfo(photo.Path);
                if (fileInfo.Exists)
                    fileInfo.Delete();

                db.Photos.Remove(photo);
                db.SaveChanges();
                if (ProductId != null)
                    return RedirectToAction("Edit", "Products", new { id = ProductId });
                return RedirectToAction("Edit", "Posts", new { id = PostId });
            }

            TempData["message"] = "Acces interzis";
            return RedirectToRoute("/");
        }

    }
}