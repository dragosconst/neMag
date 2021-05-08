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
        //[Authorize(Roles = "User, Colaborator, Admin")]
        //[HttpPut]
        public static void UploadPhotos(HttpPostedFileBase[] uploadedFiles, int Id, bool forProduct)
        {
            ApplicationDbContext db = new ApplicationDbContext();

            foreach (var uploadedFile in uploadedFiles)
            {
                if (uploadedFile == null)
                    continue;
                // Se preia numele fisierul
                string uploadedFileName = uploadedFile.FileName;
                string uploadedFileExtension = Path.GetExtension(uploadedFileName);

                // Se poate verifica daca extensia este intr-o lista dorita
                if (uploadedFileExtension == ".png" || uploadedFileExtension == ".jpg")
                {
                    // Se stocheaza fisierul in folderul Files (folderul trebuie creat in proiect)

                    
                    
                    


                    // 1. Se seteaza calea folderului de upload
                   // string uploadFolderPath = HostingEnvironment.MapPath("~/Photos/");

                    // 2. Se salveaza fisierul in acel folder  
                    //uploadedFile.SaveAs(Path.Combine(uploadFolderPath, uploadedFileName));

                    // 3. Se face o instanta de model si se populeaza cu datele necesare
                    Photo file = new Photo();
                    file.Extension = uploadedFileExtension;
                    file.Name = uploadedFileName;

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

                    string path2 = path.Remove(0, 1);
                    file.Path = Path.Combine(path.Remove(0, 1), uploadedFileName);
                    if (!System.IO.Directory.Exists(HostingEnvironment.MapPath(path)))
                        System.IO.Directory.CreateDirectory(HostingEnvironment.MapPath(path));

                    string uploadFolderPath = HostingEnvironment.MapPath(path);
                    uploadedFile.SaveAs(Path.Combine(uploadFolderPath, uploadedFileName));


                    // 4. Se adauga modelul in baza de date
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
            //if (User.IsInRole("Collaborator") && photo.UserId != User.Identity.GetUserId())
            //{
            //     TempData["message"] = "Acces interzis";
            //    return RedirectToRoute("/");
            //}
            var ProductId = photo.ProductId;
            var PostId = photo.PostId;
            
            db.Photos.Remove(photo);
            db.SaveChanges();
            if (ProductId != null)
                return RedirectToAction("Edit", "Products", new { id = ProductId });

            return RedirectToAction("Edit", "Posts", new { id = PostId });
        }

    }
}