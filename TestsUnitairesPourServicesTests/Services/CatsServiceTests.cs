using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Humanizer;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TestsUnitairesPourServices.Data;
using TestsUnitairesPourServices.Exceptions;
using TestsUnitairesPourServices.Models;
using TestsUnitairesPourServices.Services;

namespace TestsUnitairesPourServices.Services.Tests
{
    [TestClass()]
    public class CatsServiceTests
    {
        private ApplicationDBContext db;

        [TestInitialize]
        public void Init()
        {
            // En utilisant un nom différent à chaque fois, on n'a pas besoin de retirer les données
            string dbName = "CardsService" + Guid.NewGuid().ToString();
            // TODO On initialise les options de la BD, on utilise une InMemoryDatabase
            DbContextOptions<ApplicationDBContext> options = new DbContextOptionsBuilder<ApplicationDBContext>()
                // TODO il faut installer la dépendance Microsoft.EntityFrameworkCore.InMemory
                .UseInMemoryDatabase(databaseName: dbName)
                .UseLazyLoadingProxies(true) // Active le lazy loading
                .Options;

            db = new ApplicationDBContext(options);

            House[] houses =
            {
                new House
                {
                    Id = 1,
                    Address = "15 chemin de la suite",
                    OwnerName = "Hilton"
                }, new House
                {
                    Id = 2,
                    Address = "123 boulevard enceinte",
                    OwnerName = "Cuddy"
                }
            };

            Cat[] cats =
            {
                new Cat
                {
                    Id = 1,
                    Name = "Zack",
                    Age = 5,
                    House = houses[0]
                }, new Cat
                {
                    Id = 2,
                    Name = "Cody",
                    Age = 5,
                    House = houses[1]
                }, new Cat
                {
                    Id = 3,
                    Name = "Dr House",
                    Age = 14
                }
            };

            db.AddRange(houses);
            db.AddRange(cats);
            db.SaveChanges();
        }

        [TestCleanup]
        public void Dispose()
        {
            db.Dispose();
        }

        [TestMethod()]
        public void MoveTestReussi()
        {
            CatsService catsService = new CatsService(db);

            Cat cat = db.Cat.Find(1);
            House oldHouse = cat.House;
            House newHouse = db.House.Find(2);
            Cat movedCat = catsService.Move(1, oldHouse, newHouse);
            Assert.AreEqual(cat, movedCat);
        }

        [TestMethod()]
        public void MoveTestChatInconnu()
        {
            CatsService catsService = new CatsService(db);

            House oldHouse = db.House.Find(1);
            House newHouse = db.House.Find(2);
            Cat movedCat = catsService.Move(-1, oldHouse, newHouse);
            Assert.AreEqual(null, movedCat);
        }

        [TestMethod()]
        public void MoveTestChatErrant()
        {
            CatsService catsService = new CatsService(db);

            House oldHouse = db.House.Find(1);
            House newHouse = db.House.Find(2);

            Exception e = Assert.ThrowsException<WildCatException>(() => catsService.Move(3, oldHouse, newHouse));
            Assert.AreEqual("On n'apprivoise pas les chats sauvages", e.Message);
        }

        [TestMethod()]
        public void MoveTestChatMauvaiseMaison()
        {
            CatsService catsService = new CatsService(db);

            House oldHouse = db.House.Find(1);
            House newHouse = db.House.Find(2);

            Exception e = Assert.ThrowsException<DontStealMyCatException>(() => catsService.Move(2, oldHouse, newHouse));
            Assert.AreEqual("Touche pas à mon chat!", e.Message);
        }
    }
}