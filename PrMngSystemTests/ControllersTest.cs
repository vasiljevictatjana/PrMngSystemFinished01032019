using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Web.Mvc;
using PrMngSystem.Controllers;
using PrMngSystem.Models;
using Moq;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Web;
using System.Data.Entity;

namespace PrMngSystemTests
{
    [TestClass]
    public class ControllersTest
    {


        [TestMethod]
        public void Index()
        {
            //Arrange
            HomeController controller = new HomeController();

            //Act
            ViewResult result = controller.Index() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void AboutPass()
        {
            //Arrange
            HomeController controller = new HomeController();

            //Act
            ViewResult result = controller.About() as ViewResult;

            //Assert
            Assert.AreEqual("Your application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void AboutFail()
        {
            //Arrange
            HomeController controller = new HomeController();

            //Act
            ViewResult result = controller.About() as ViewResult;

            //Assert
            Assert.AreEqual("application description page.", result.ViewBag.Message);
        }

        [TestMethod]
        public void Contact()
        {
            //Arrange
            HomeController controller = new HomeController();

            //Act
            ViewResult result = controller.Contact() as ViewResult;

            //Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void CreateTask()
        {
            var mockSet = new Mock<DbSet<Task>>();

            var mockContext = new Mock<PrMngSystemDBEntities>();
            mockContext.Setup(m => m.Tasks).Returns(mockSet.Object);

            var service = new TaskController();

            var task = new Task
            {
                progress = (decimal)12.00,
                deadline = DateTime.Parse("12-Mar-19 12:00:00 AM"),
                description = "task desc",
                status = 2,
                assignee = null
            };

            var result = service.CreateTask(1, task);

            Assert.IsNotNull(result);
        }
    }
}
