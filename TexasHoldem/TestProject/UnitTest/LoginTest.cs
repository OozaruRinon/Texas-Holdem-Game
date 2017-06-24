﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SL;
using Moq;
//using DAL;
using Backend.User;
using Backend;
using System.Collections.Generic;
using ApplicationFacade;
using Backend.Game;
using Backend.Game.DecoratorPreferences;
using static Backend.Game.DecoratorPreferences.GamePolicyDecPref;
using System;
using Database;

namespace TestProject.UnitTest
{
    [TestClass]
    public class LoginTest
    {
        private SLInterface sl;
        private IDB db;
        private GameCenter center;

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
            db = new DBImpl();
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
                //new SystemUser("Hadas", "email0", "image0", 1000),
                //new SystemUser("Gili", "email1", "image1", 0),
                //new SystemUser("Or", "email2", "image2", 700),
                //new SystemUser("Aviv", "email3", "image3", 1500)
            };

            center = GameCenter.getGameCenter();

            ////set users ranks.
            //userList[0].rank = 10;
            //userList[1].rank = 15;
            //userList[2].rank = 20;
            //userList[3].rank = 25;

            //for (int i = 0; i < 4; i++)
            //{
            //    userList[i].id = i;
            //    center.loggedInUsers.Add(userList[i]);
            //    //center.login(userList[i].name, userList[i].password);
            //}


            //set the leagues
            center.maintainLeagues(userList);

            //get the league of user 3
            League l = center.getUserLeague(userList[3]);

            //setting the games
            //pref order: mustpref(spectate, league)->game type , buy in policy, starting chips, minimal bet, minimum players, maximum players.
            var gamesList = new List<TexasHoldemGame>
                         {
                //regular games
                new TexasHoldemGame(userList[0],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (9,null) ))))),true),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[0],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (9,null) ))))),false),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[1],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),true),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[1],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[2],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[2],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                //league games
                new TexasHoldemGame(userList[3],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false,l.minRank,l.maxRank),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2])),
                new TexasHoldemGame(userList[3],new MustPreferences(new GamePolicyDecPref(GameTypePolicy.No_Limit,0,
                                                                    new BuyInPolicyDecPref(100,new StartingAmountChipsCedPref(500,
                                                                    new MinBetDecPref(20,new MinPlayersDecPref(2,
                                                                    new MaxPlayersDecPref (2,null) ))))),false,l.minRank,l.maxRank),
                                                                    userIdDeltaRank => db.EditUserById(userIdDeltaRank[0], null, null, null, null, null, userIdDeltaRank[1], false),
                                                                    userIdLeaderB => db.EditUserLeaderBoardsById(userIdLeaderB[0], userIdLeaderB[1], userIdLeaderB[2]))
            //new TexasHoldemGame(userList[0], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 9, true)),
            //    new TexasHoldemGame(userList[0], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 9, false)),
            //    new TexasHoldemGame(userList[1], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, true)),
            //    new TexasHoldemGame(userList[1], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[2], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[2], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false)),
            //    new TexasHoldemGame(userList[3], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false, 0, 1000)),
            //    new TexasHoldemGame(userList[3], new GamePreferences(GamePreferences.GameTypePolicy.no_limit, 100, 500, 20, 2, 2, false, 1000, 2000))
                         };

            for (int i = 0; i < gamesList.Count; i++)
            {
                gamesList[i].gameId = i;
                center.texasHoldemGames.Add(gamesList[i]);
            }

            //Mock<DALInterface> dalMock = new Mock<DALInterface>();
            //dalMock.Setup(x => x.getAllGames()).Returns(gamesList);
            //dalMock.Setup(x => x.getUserById(It.IsAny<int>())).Returns((int i) => userList[i]);
            //dalMock.Setup(x => x.getGameById(It.IsAny<int>())).Returns((int i) => gamesList[i]);
            sl = new SLImpl();
        }

        [TestMethod]
        public void successLoginTest()
        {
            SystemUser user = db.getUserByName("test2");
            sl.removeUser(4, user.id);
            sl.removeUser(5, user.id);
            sl.Logout(db.getUserByName("test2").id);
            object m = sl.Login("test2", "2");
            Assert.IsInstanceOfType(m, typeof(SystemUser));
        }

        [TestMethod]
        public void successRegisterTest()
        {
            object m = sl.Register("crash bandicoot", "green boxes are the worst", "crash@bash.bugabugabuga", null);
            Assert.IsInstanceOfType(m, typeof(SystemUser));
            Assert.AreEqual(((SystemUser)m).name, "crash bandicoot");
            db.deleteUser(db.getUserByName("crash bandicoot").id);
        }

        //[TestMethod]
        //[ExpectedException(typeof(ArgumentException), "The user is already logged in")]
        //public void alreadyLoggedInTest()
        //{
        //    sl.Register("crash bandicoottt", "green boxes are the worst", "crash@bash.bugabugabuga", "hedgehog photo");
            
        //    object m = sl.Login("crash bandicoottt", "green boxes are the worst");
        //    db.deleteUser(db.getUserByName("crash bandicoottt").id);
        //}

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No such user.")]
        public void notRegisteredTest()
        {
            object o = sl.Login("a worm from worms world party", "for a donkey, for a donkey, my kindom for a donkey");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No such user.")]
        public void emptyUserNameTest()
        {
            object m = sl.Login("", "Transparancy is overwhelming");
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException), "No such user.")]
        public void emptyPasswordTest()
        {
            object m = sl.Login("sandro the living dead", "");
        }
    }
}
