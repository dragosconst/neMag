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
        //private Models.ApplicationDbContext db = new Models.ApplicationDbContext();
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
                    string uploadFolderPath = HostingEnvironment.MapPath("~/Photos/");

                    // 2. Se salveaza fisierul in acel folder  
                    uploadedFile.SaveAs(Path.Combine(uploadFolderPath, uploadedFileName));

                    // 3. Se face o instanta de model si se populeaza cu datele necesare
                    Photo file = new Photo();
                    file.Extension = uploadedFileExtension;
                    file.Name = uploadedFileName;
                    file.Path = Path.Combine("/Photos/", uploadedFileName);
                    if (forProduct)
                        file.ProductId = Id;
                    else
                        file.PostId = Id;

                    // 4. Se adauga modelul in baza de date
                    db.Photos.Add(file);
                }
            }

            db.SaveChanges();
        }
    }
}