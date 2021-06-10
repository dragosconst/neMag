using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using neMag.Controllers;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;

namespace neMag.Tests.Controllers
{
    [TestClass]
    public class ProductsControllerTest
    {
        [TestMethod]
        public void GetAllCategories_CategoryIdList_ReturnSameIds()
        {
            ProductsController productController = new ProductsController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
               new Category
               {
                   CategoryId = 1,
                   Title = "1"
               },
               new Category
               {
                   CategoryId = 2,
                   Title = "2"
               },
               new Category
               {
                   CategoryId = 3,
                   Title = "3"
               }
            };
            IQueryable<Category> queryableCats = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCats.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCats.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCats.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCats.GetEnumerator());

            mockDbconnection.Setup(d => d.Categories).Returns(mockCategories.Object);

            var mockContext = new Mock<ControllerContext>();
            productController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(productController);
            po.SetField("db", mockDbconnection.Object);
            List<string> categoryIds = categories.Select(c => c.CategoryId.ToString()).ToList();

            List<string> result = productController.GetAllCategories().OrderBy(s => s.Value).Select(s => s.Value).ToList();

            Assert.AreEqual(String.Concat(result), String.Concat(categoryIds));
        }


    }
}
