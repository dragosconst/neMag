using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using neMag.Controllers;
using neMag.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
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


        /*
         * Check that the admin gets the ViewBag show buttons field set to true.
         * **/
        [TestMethod]
        public void SetAccessRights_AdminUser_GetsAllButtons()
        {
            ProductsController productController = new ProductsController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole("Admin")).Returns(true);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            productController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(productController);

            po.Invoke("SetAccessRights", args: new Product());
            
            Assert.IsTrue(productController.ViewData.ContainsKey("showButtons"));
            var val = productController.ViewData["showButtons"];
            Assert.AreEqual(val, true);
            Assert.IsTrue(productController.ViewData.ContainsKey("isCollaborator"));
            val = productController.ViewData["isCollaborator"];
            Assert.AreEqual(val, false);
        }

        /*
         * Check that collaborators only get their respective buttons.
         * **/
        [TestMethod]
        public void SetAccessRights_CollaboratorUser_GetsColButtons()
        {
            ProductsController productController = new ProductsController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole("Collaborator")).Returns(true);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            productController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(productController);

            po.Invoke("SetAccessRights", args: new Product
            {
                UserId = username
            });

            Assert.IsTrue(productController.ViewData.ContainsKey("showButtons"));
            var val = productController.ViewData["showButtons"];
            Assert.AreEqual(val, true);
            Assert.IsTrue(productController.ViewData.ContainsKey("isCollaborator"));
            val = productController.ViewData["isCollaborator"];
            Assert.AreEqual(val, true);
        }

        /*
         * Check that regular users don't get any edit\delete buttons
         * shown for any product.
         * **/
        [TestMethod]
        public void SetAccessRights_RegularUser_GetsNoButtons()
        {
            ProductsController productController = new ProductsController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            productController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(productController);

            po.Invoke("SetAccessRights", args: new Product());

            Assert.IsTrue(productController.ViewData.ContainsKey("showButtons"));
            var val = productController.ViewData["showButtons"];
            Assert.AreEqual(val, false);
            Assert.IsTrue(productController.ViewData.ContainsKey("isCollaborator"));
            val = productController.ViewData["isCollaborator"];
            Assert.AreEqual(val, false);
        }


        /*
         * Check that collaborators can't edit the buttons of the products that
         * belong to other collaborators.
         * **/
        [TestMethod]
        public void SetAccessRights_CollaboratorUserOtherProduct_GetsNoButtons()
        {
            ProductsController productController = new ProductsController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole("Collaborator")).Returns(true);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            productController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(productController);

            po.Invoke("SetAccessRights", args: new Product());

            Assert.IsTrue(productController.ViewData.ContainsKey("showButtons"));
            var val = productController.ViewData["showButtons"];
            Assert.AreEqual(val, false);
            Assert.IsTrue(productController.ViewData.ContainsKey("isCollaborator"));
            val = productController.ViewData["isCollaborator"];
            Assert.AreEqual(val, false);
        }


    }
}
