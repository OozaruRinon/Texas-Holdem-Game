﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace Backend.System
{
	public class GameCenter : Messages.Notification
	{
		private List<Game.TexasHoldemGame> texasHoldemGames;
        private List<Game.League> leagues;

		public Game.TexasHoldemGame createRegularGame(int userId, Game.GamePreferences preferences)
		{
			var game = new Game.TexasHoldemGame(userId, preferences);
            texasHoldemGames.Add(game);
            return game;

        }

        public Game.TexasHoldemGame createLeagueGame(int userId, Game.GamePreferences preferences, Guid leagueId)
        {
            var league = leagues.Where(l => l.leagueId == leagueId).Single();
            var leagueGame = new Game.LeagueTexasHoldemGame(userId, preferences, league);
            texasHoldemGames.Add(leagueGame);
            return leagueGame;
        }

        public void maintainLeagues(List<User.SystemUser> users)
        {
            foreach(var u in users)
            {
                foreach(var l in leagues)
                {
                    if (u.rank >= l.minRank && u.rank <= l.maxRank && !l.isUserInLeague(u))
                    {
                        l.addUser(u);
                    }
                    else if ((u.rank < l.minRank || u.rank > l.maxRank) && l.isUserInLeague(u))
                    {
                        l.removeUser(u);
                    }
                }
            }
        }

        public List<Game.TexasHoldemGame> findGameByCriteria(String str)
		{
			return null;
		}

        public Game.League createLeague(int minRank, int maxRank)
        {
            return new Game.League(minRank, maxRank);
        }
	}
}
