using Microsoft.VisualStudio.TestTools.UnitTesting;
using neMag.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace neMag.Tests.Controllers
{
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void GetAllCategories_CategoryIdList()
        {
            ProductsController productController = new ProductsController();
            Models.ApplicationDbContext db = new Models.ApplicationDbContext();
            List<string> categoryIds = db.Categories.OrderBy(c => c.CategoryId).Select(c => c.CategoryId.ToString()).ToList();

            List<string> result = productController.GetAllCategories().OrderBy(s => s.Value).Select(s => s.Value).ToList();

            Assert.AreEqual(String.Concat(result), String.Concat(categoryIds));
        }
    }
}
