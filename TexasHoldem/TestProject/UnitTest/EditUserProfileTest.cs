﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL;
using Backend.User;
using System.Collections.Generic;
using Backend;
using ApplicationFacade;
using PeL;

namespace TestProject
{
    [TestClass]
    public class EditUserProfileTest
    {
        SLInterface sl;
        private IPeL db;
        GameCenter center = GameCenter.getGameCenter();

        [TestCleanup]
        public void Cleanup()
        {
            for (int i = 0; i < 4; i++)
                db.deleteUser(db.getUserByName("test" + i).id);

            center.shutDown();
        }

        [TestInitialize]
        public void SetUp()
        {
            db = new PeLImpl();
            for (int i = 0; i < 4; i++)
            {
                db.RegisterUser("test" + i, "" + i, "email" + i, null);
            }
            db.EditUserById(db.getUserByName("test0").id, null, null, null, null, 1000, 10, false);
            db.EditUserById(db.getUserByName("test1").id, null, null, null, null, 0, 15, false);
            db.EditUserById(db.getUserByName("test2").id, null, null, null, null, 700, 20, false);
            db.EditUserById(db.getUserByName("test3").id, null, null, null, null, 1500, 25, false);


            var userList = new List<SystemUser>
            {
                db.getUserByName("test0"),
                db.getUserByName("test1"),
                db.getUserByName("test2"),
                db.getUserByName("test3")
            };

            center = GameCenter.getGameCenter();

            sl = new SLImpl();
        }

        [TestMethod]
        public void successEditUserTest()
        {
            object obj = sl.editUserProfile(db.getUserByName("test0").id, "Hadas123", "12345", "email7", null, 100);
            Assert.IsInstanceOfType(obj, typeof(ReturnMessage));
            Assert.IsTrue(((ReturnMessage)obj).success);
            sl.editUserProfile(db.getUserByName("Hadas123").id, "test0", "12345", "email7", null, 100);
        }

        [TestMethod]
        public void alreadyExistsUserNameTest()
        {
            object obj = sl.editUserProfile(db.getUserByName("test0").id, "test3", "12345", "email7", null, 100);
            Assert.IsInstanceOfType(obj, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)obj).success);
        }

        [TestMethod]
        public void editUserEmptyUserNameTest()
        {
            object obj = sl.editUserProfile(db.getUserByName("test0").id, "", "12345", "email7", null, 100);
            Assert.IsInstanceOfType(obj, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)obj).success);
        }

        [TestMethod]
        public void alreadyExistsEmailTest()
        {
            object obj = sl.editUserProfile(db.getUserByName("test0").id, "gil", "1111", "email3", null, 100);
            Assert.IsInstanceOfType(obj, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)obj).success);
        }

        [TestMethod]
        public void negativeMoneyTest()
        {
            object obj = sl.editUserProfile(db.getUserByName("test0").id, "gil", "1111", "email100", null, -100);
            Assert.IsInstanceOfType(obj, typeof(ReturnMessage));
            Assert.IsFalse(((ReturnMessage)obj).success);
        }
    }
}
