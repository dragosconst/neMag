using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Security.Claims;
using System.Security.Principal;
using System.Web;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using neMag.Controllers;
using neMag.Models;
using Rhino.Mocks;

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
        public void GetCart_EmptyUser_CreateNewCart()
        {
            CartController cosController = new CartController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            Product demoProduct = new Product
            {
                ProductId = 1,
                CategoryId = 1,
                ProductName = "demo",
                Price = 5,
                Discount = 0,
                Accepted = true
            };
            Order demoOrder = new Order
            {
                OrderId = 1,
                UserId = ""
            };
            OrderContent demoOc = new OrderContent
            {
                Product = demoProduct,
                Order = demoOrder,
                Quantity = 1,
                Total = 0.0
            };
            List<OrderContent> fakeOrderContents = new List<OrderContent>
            {
                demoOc
            };
            List<Order> fakeOrders = new List<Order>
            {
                demoOrder
            };
            IQueryable<OrderContent> queryableList = fakeOrderContents.AsQueryable();
            IQueryable<Order> queryableOrders = fakeOrders.AsQueryable();

            var mockOCSet = new Mock<DbSet<OrderContent>>();
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());

            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryableOrders.Provider);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryableOrders.Expression);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryableOrders.ElementType);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(queryableOrders.GetEnumerator());

            mockDbconnection.Setup(d => d.Products.Find(1)).
                Returns(demoProduct);
            mockDbconnection.Setup(d => d.OrderContents).
                Returns(mockOCSet.Object);
            mockDbconnection.Setup(d => d.Orders).
                Returns(mockOrderSet.Object);
            mockDbconnection.Setup(d => d.SaveChanges()).Returns(null);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(cosController);
            po.SetField("db", mockDbconnection.Object);
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
         * Checks if the behaviour on a user that already has a cart in the db
         * is the expected one: get cart should return it.
         * **/
        [TestMethod]
        public void GetCart_AlreadyHasCart_ReturnCart()
        {
            CartController cosController = new CartController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            Order demoOrder = new Order
            {
                OrderId = 1,
                UserId = username,
                Status = "Cart"
            };
            List<Order> fakeOrders = new List<Order>
            {
                demoOrder
            };
            IQueryable<Order> queryableOrders = fakeOrders.AsQueryable();

            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryableOrders.Provider);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryableOrders.Expression);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryableOrders.ElementType);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(queryableOrders.GetEnumerator());

            mockDbconnection.Setup(d => d.Orders).
                Returns(mockOrderSet.Object);
            mockDbconnection.Setup(d => d.SaveChanges()).Returns(null);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(cosController);
            po.SetField("db", mockDbconnection.Object);
            Order expectedResult = demoOrder;


            Order result = (Order)po.Invoke("GetCart");

            Assert.AreEqual(expectedResult.UserId, result.UserId);
            Assert.AreEqual(expectedResult.Status, result.Status);

        }


        /*
         * Test that AddToOrder doesn't fail to redirect unauthenticated
         * users.
         * **/
        [TestMethod]
        public void AddToOrder_UnauthenicatedUser()
        {
            CartController cosController = new CartController();

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity.IsAuthenticated).Returns(false);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;


            var result = cosController.AddToOrder(0);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirect = (RedirectToRouteResult)result;
            Assert.AreEqual("Account", redirect.RouteValues["controller"]);
            Assert.AreEqual("Login", redirect.RouteValues["action"]);
        }

        /*
         * Checks if an authenticated order gets the expected result
         * when they add to their order a product which was already
         * present in the order (it should increase its quantity and
         * redirect to Index).
         * **/
        [TestMethod]
        public void AddToOrder_AuthenicatedUserProductAlreadyOrdered_IncreaseQuantityInOrderContent()
        {
            CartController cosController = new CartController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            Product demoProduct = new Product
            {
                ProductId = 1,
                CategoryId = 1,
                ProductName = "demo",
                Price = 5,
                Discount = 0,
                Accepted = true,
                Stock = 1
            };
            Order demoOrder = new Order
            {
                OrderId = 1,
                UserId = username,
                Status = "Cart",
                TotalPrice = 5
            };
            OrderContent demoOc = new OrderContent
            {
                Product = demoProduct,
                Order = demoOrder,
                Quantity = 1,
                Total = 0.0
            };
            List<OrderContent> fakeOrderContents = new List<OrderContent>
            {
                demoOc
            };
            demoOrder.OrderContents = fakeOrderContents;
            List<Order> fakeOrders = new List<Order>
            {
                demoOrder
            };
            IQueryable<OrderContent> queryableList = fakeOrderContents.AsQueryable();
            IQueryable<Order> queryableOrders = fakeOrders.AsQueryable();

            var mockOCSet = new Mock<DbSet<OrderContent>>();
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());

            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryableOrders.Provider);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryableOrders.Expression);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryableOrders.ElementType);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(queryableOrders.GetEnumerator());

            mockDbconnection.Setup(d => d.Products.Find(1)).
                Returns(demoProduct);
            mockDbconnection.Setup(d => d.OrderContents).
                Returns(mockOCSet.Object);
            mockDbconnection.Setup(d => d.Orders).
                Returns(mockOrderSet.Object);
            mockDbconnection.Setup(d => d.SaveChanges()).Returns(null);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;
            cosController.ValueProvider = new DictionaryValueProvider<object>(
    new Dictionary<string, object>() { { "OrderContent", demoOc } }, null);
            PrivateObject po = new PrivateObject(cosController);
            po.SetField("db", mockDbconnection.Object);

            var result = cosController.AddToOrder(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirect = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirect.RouteValues["action"]);
            Assert.AreEqual(demoOc.Quantity, 2);
        }

        /*
         * Check if adding a new product to the cart creates a corresponding
         * OrderContent object correctly.
         * **/
        [TestMethod]
        public void AddToOrder_AuthenicatedUserProductNotOrdered_CreateNewOrderContent()
        {
            CartController cosController = new CartController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            Product demoProduct1 = new Product
            {
                ProductId = 2,
                CategoryId = 1,
                ProductName = "demo",
                Price = 5,
                Discount = 0,
                Accepted = true
            };
            Product demoProduct2 = new Product
            {
                ProductId = 1,
                CategoryId = 1,
                ProductName = "demo2",
                Price = 50,
                Discount = 0,
                Accepted = true,
                Stock = 1
            };
            Order demoOrder = new Order
            {
                OrderId = 1,
                UserId = username,
                Status = "Cart"
            };
            OrderContent demoOc = new OrderContent
            {
                Product = demoProduct1,
                Order = demoOrder,
                Quantity = 1,
                Total = 0.0
            };
            List<OrderContent> fakeOrderContents = new List<OrderContent>
            {
                demoOc
            };
            demoOrder.OrderContents = fakeOrderContents;
            List<Order> fakeOrders = new List<Order>
            {
                demoOrder
            };
            IQueryable<OrderContent> queryableList = fakeOrderContents.AsQueryable();
            IQueryable<Order> queryableOrders = fakeOrders.AsQueryable();

            var mockOCSet = new Mock<DbSet<OrderContent>>();
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());
            mockOCSet.Setup(d => d.Add(It.IsAny<OrderContent>())).Callback<OrderContent>((s) => fakeOrderContents.Add(s));

            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryableOrders.Provider);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryableOrders.Expression);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryableOrders.ElementType);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(queryableOrders.GetEnumerator());

            mockDbconnection.Setup(d => d.Products.Find(1)).
                Returns(demoProduct2);
            mockDbconnection.Setup(d => d.OrderContents).
                Returns(mockOCSet.Object);
            mockDbconnection.Setup(d => d.Orders).
                Returns(mockOrderSet.Object);
            mockDbconnection.Setup(d => d.SaveChanges()).Returns(null);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;
            cosController.ValueProvider = new DictionaryValueProvider<object>(
    new Dictionary<string, object>() { { "OrderContent", demoOc } }, null);
            PrivateObject po = new PrivateObject(cosController);
            po.SetField("db", mockDbconnection.Object);

            var result = cosController.AddToOrder(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirect = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirect.RouteValues["action"]);
            Assert.AreEqual(mockOCSet.Object.Count(), 3); // 3 because db.OrderContents and cart.OrderContents are the same in this context
        }

        /*
         * Check if trying to order an unaccepted product has the expected behaviour.
         * **/
        [TestMethod]
        public void AddToOrder_AuthenicatedUserProductNotAccepted_RedirectToIndexAndErrMsg()
        {
            CartController cosController = new CartController();
            var username = "fakeuser";
            var identity = new GenericIdentity(username, "");
            var nameIdentifierClaim = new Claim(ClaimTypes.NameIdentifier, username);
            identity.AddClaim(nameIdentifierClaim);

            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            Product demoProduct = new Product
            {
                ProductId = 1,
                CategoryId = 1,
                ProductName = "demo",
                Price = 5,
                Discount = 0,
                Accepted = false,
                Stock = 1
            };
            Order demoOrder = new Order
            {
                OrderId = 1,
                UserId = username,
                Status = "Cart"
            };
            OrderContent demoOc = new OrderContent
            {
                Product = demoProduct,
                Order = demoOrder,
                Quantity = 1,
                Total = 0.0
            };
            List<OrderContent> fakeOrderContents = new List<OrderContent>
            {
                demoOc
            };
            demoOrder.OrderContents = fakeOrderContents;
            List<Order> fakeOrders = new List<Order>
            {
                demoOrder
            };
            IQueryable<Order> queryableOrders = fakeOrders.AsQueryable();
            IQueryable<OrderContent> queryableList = fakeOrderContents.AsQueryable();

            var mockOCSet = new Mock<DbSet<OrderContent>>();
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Provider).Returns(queryableList.Provider);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.Expression).Returns(queryableList.Expression);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.ElementType).Returns(queryableList.ElementType);
            mockOCSet.As<IQueryable<OrderContent>>().Setup(m => m.GetEnumerator()).Returns(queryableList.GetEnumerator());
            mockOCSet.Setup(d => d.Add(It.IsAny<OrderContent>())).Callback<OrderContent>((s) => fakeOrderContents.Add(s));

            var mockOrderSet = new Mock<DbSet<Order>>();
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Provider).Returns(queryableOrders.Provider);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.Expression).Returns(queryableOrders.Expression);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.ElementType).Returns(queryableOrders.ElementType);
            mockOrderSet.As<IQueryable<Order>>().Setup(m => m.GetEnumerator()).Returns(queryableOrders.GetEnumerator());


            mockDbconnection.Setup(d => d.Products.Find(1)).
                Returns(demoProduct);
            mockDbconnection.Setup(d => d.Orders).
                Returns(mockOrderSet.Object);
            mockDbconnection.Setup(d => d.OrderContents).
                Returns(mockOCSet.Object);

            var mockPrincipal = new Mock<IPrincipal>();
            mockPrincipal.Setup(x => x.Identity).Returns(identity);

            var mockContext = new Mock<ControllerContext>();
            mockContext.Setup(p => p.HttpContext.User).Returns(mockPrincipal.Object);
            cosController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(cosController);
            po.SetField("db", mockDbconnection.Object);

            var result = cosController.AddToOrder(1);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            RedirectToRouteResult redirect = (RedirectToRouteResult)result;
            Assert.AreEqual("Index", redirect.RouteValues["action"]);
            Assert.IsTrue(cosController.TempData.ContainsKey("message"));
            var msg = cosController.TempData["message"];
            Assert.AreEqual(msg, "Produsul nu poate fi comandat");
        }

    }
}
