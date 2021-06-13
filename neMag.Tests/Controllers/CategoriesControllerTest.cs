using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using neMag.Controllers;
using neMag.Models;

namespace neMag.Tests.Controllers
{
    [TestClass]
    public class CategoriesControllerTest
    {

        /*
         * Check if GetAllCategories returns a list of all categories when called
         * with a null argument.
         * **/
        [TestMethod]
        public void GetAllCategories_NullArgument_ListOfAllCategories()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories    = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Title = "cat3",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 4,
                    Title = "cat4",
                    ParentId = 2
                }
            };
            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);

            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = po.Invoke("GetAllCategories", new object[] { null });

            Assert.IsInstanceOfType(result, typeof(IEnumerable<SelectListItem>));
            var listResult = (IEnumerable<SelectListItem>)result;
            Assert.AreEqual(listResult.Count(), categories.Count() + 1);
        }

        /*
         * Check if calling GetAllCategories with a category removes from the 
         * options the category itself and all its parents.
         * **/
        [TestMethod]
        public void GetAllCategories_CatArgument_AllCategoriesButThisOneAndSuccesors()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Title = "cat3",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 4,
                    Title = "cat4",
                    ParentId = 2
                },
                new Category
                {
                    CategoryId = 5,
                    Title = "cat5",
                    ParentId = 3
                }
            };
            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());
            mockCategories.Setup(set => set.Find(It.IsAny<int>())).Returns<object[]>(category => categories.FirstOrDefault(c => c.CategoryId == (int)category[0])); // honestly no idea how this works

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);


            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = po.Invoke("GetAllCategories", new object[] { categories[0] });

            Assert.IsInstanceOfType(result, typeof(IEnumerable<SelectListItem>));
            var listResult = (IEnumerable<SelectListItem>)result;
            Assert.AreEqual(listResult.Count(), 3);
            foreach(SelectListItem item in listResult)
            {
                Assert.AreNotEqual(item.Value, "1");
                Assert.AreNotEqual(item.Value, "2");
                Assert.AreNotEqual(item.Value, "4");
            }
        }


        /*
         * Checks if the behaviour on unrelated categories is the expected
         * one.
         * **/
        [TestMethod]
        public void IsParent_NotRelated_ReturnsFalse()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = -1
                }
            };

            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());
            mockCategories.Setup(set => set.Find(It.IsAny<int>())).Returns<object[]>(category => categories.FirstOrDefault(c => c.CategoryId == (int)category[0])); // honestly no idea how this works

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);

            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = po.Invoke("IsParent", new object[] { categories[0].CategoryId, categories[1].CategoryId });

            Assert.IsInstanceOfType(result, typeof(bool));
            var boolResult = (bool)result;
            Assert.IsFalse(boolResult);
        }

        /*
         * Checks if the method returns true on related categories
         * **/
        [TestMethod]
        public void IsParent_AreRelated_ReturnsTrue()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Title = "cat3",
                    ParentId = 2
                }
            };

            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());
            mockCategories.Setup(set => set.Find(It.IsAny<int>())).Returns<object[]>(category => categories.FirstOrDefault(c => c.CategoryId == (int)category[0])); // honestly no idea how this works

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);

            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = po.Invoke("IsParent", new object[] { categories[2].CategoryId, categories[1].CategoryId });

            Assert.IsInstanceOfType(result, typeof(bool));
            var boolResult = (bool)result;
            Assert.IsTrue(boolResult);
        }


        /*
         * Checks if the new method works as expected on categories with
         * no parents.
         * **/
        [TestMethod]
        public void New_NoParentCat_NoFinds()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Title = "cat3",
                    ParentId = 2
                }
            };
            Category newCategory = new Category
            {
                CategoryId = 4,
                Title = "cat4",
                ParentId = 0
            };

            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());
            mockCategories.Setup(set => set.Find(It.IsAny<int>())).Returns<object[]>(category => categories.FirstOrDefault(c => c.CategoryId == (int)category[0])); // honestly no idea how this works
            mockCategories.Setup(d => d.Add(It.IsAny<Category>())).Callback<Category>((s) => categories.Add(s));

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);
            mockDbconnection.Setup(db => db.SaveChanges()).Returns(null);

            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = categoriesController.New(newCategory);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            mockDbconnection.Verify(mock => mock.SaveChanges(), Times.Once()); // checks how many times the method was called
            mockCategories.Verify(mock => mock.Find(It.IsAny<int>()), Times.Never());
            mockCategories.Verify(mock => mock.Add(It.IsAny<Category>()), Times.Once());
            Assert.AreEqual(categories.Count(), 4);
        }


        [TestMethod]
        public void New_HasParentCat_OneFind()
        {
            CategoriesController categoriesController = new CategoriesController();
            var mockDbconnection = new Mock<Models.ApplicationDbContext>();
            List<Category> categories = new List<Category>
            {
                new Category
                {
                    CategoryId = 1,
                    Title = "cat1",
                    ParentId = -1
                },
                new Category
                {
                    CategoryId = 2,
                    Title = "cat2",
                    ParentId = 1
                },
                new Category
                {
                    CategoryId = 3,
                    Title = "cat3",
                    ParentId = 2
                }
            };
            Category newCategory = new Category
            {
                CategoryId = 4,
                Title = "cat4",
                ParentId = 2
            };

            IQueryable<Category> queryableCategories = categories.AsQueryable();

            var mockCategories = new Mock<DbSet<Category>>();
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Provider).Returns(queryableCategories.Provider);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.Expression).Returns(queryableCategories.Expression);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.ElementType).Returns(queryableCategories.ElementType);
            mockCategories.As<IQueryable<Category>>().Setup(m => m.GetEnumerator()).Returns(queryableCategories.GetEnumerator());
            mockCategories.Setup(set => set.Find(It.IsAny<int>())).Returns<object[]>(category => categories.FirstOrDefault(c => c.CategoryId == (int)category[0])); // honestly no idea how this works
            mockCategories.Setup(d => d.Add(It.IsAny<Category>())).Callback<Category>((s) => categories.Add(s));

            mockDbconnection.Setup(db => db.Categories).Returns(mockCategories.Object);
            mockDbconnection.Setup(db => db.SaveChanges()).Returns(null);

            var mockContext = new Mock<ControllerContext>();
            categoriesController.ControllerContext = mockContext.Object;
            PrivateObject po = new PrivateObject(categoriesController);
            po.SetField("db", mockDbconnection.Object);

            var result = categoriesController.New(newCategory);

            Assert.IsInstanceOfType(result, typeof(RedirectToRouteResult));
            mockDbconnection.Verify(mock => mock.SaveChanges(), Times.Once()); // checks how many times the method was called
            mockCategories.Verify(mock => mock.Find(It.IsAny<int>()), Times.Once());
            mockCategories.Verify(mock => mock.Add(It.IsAny<Category>()), Times.Once());
            Assert.AreEqual(categories.Count(), 4);
        }
    }
}
