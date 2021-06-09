using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using neMag.Controllers;
using neMag.Models;

namespace neMag.Tests.Controllers
{
    [TestClass]
    public class CosControllerTest
    {
        /*
         * Will create a new user, in order to check if
         * the method correctly creates an empty cart.
         * **/
        [TestMethod]
        public void GetCart_EmptyUser()
        {
            CosController cosController = new CosController();
            Models.ApplicationDbContext db = new Models.ApplicationDbContext();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole("User")).Returns(true);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;

            PrivateObject po = new PrivateObject(cosController);
            Order expectedResult = new Order
            {
                UserId = username,
                Status = "Cart",
                Date = DateTime.Now,
                OrderContents = new List<OrderContent>()
            };


            Order result = (Order)po.Invoke("GetCart");

            Assert.AreEqual(expectedResult.UserId, result.UserId);
            Assert.AreEqual(expectedResult.Status, result.Status);
        }


        /*
         * Checks if the admin getcart method works as intended.
         * I'm using the admin user just because they have the same account
         * credentials on all of our machines, so it should run alright
         * for everyone.
         * **/
        [TestMethod]
        public void GetCart_AdminUser()
        {
            CosController cosController = new CosController();
            Models.ApplicationDbContext db = new Models.ApplicationDbContext();
            string aid = db.Users.Where(u => u.Email == "admin1@gmail.com")
             .Select(u => u.Id).ToList().First();
            var identity = new GenericIdentity(aid, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, aid);
            identity.AddClaim(nameIdentifierClaim);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);
            mockPrincipal.Setup(x => x.IsInRole("Admin")).Returns(true);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;

            PrivateObject po = new PrivateObject(cosController);
            Order expectedResult = db.Orders.Where(o => o.UserId == aid && o.Status == "Cart").ToList().First();


            Order result = (Order)po.Invoke("GetCart");

            Assert.AreEqual(expectedResult.UserId, result.UserId);
            Assert.AreEqual(expectedResult.Status, result.Status);

        }
    }
}
