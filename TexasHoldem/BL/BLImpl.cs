﻿using Backend.User;
using Backend.Game;
using Backend;
using DAL;
using BL;
using System.Collections.Generic;
using System;
using System.Linq;

public class BLImpl : BLInterface
{
	private DALInterface dal;

	public BLImpl()
	{
		dal = new DALDummy();
	}

    public BLImpl(DALInterface dal)
    {
        this.dal = dal;
    }
	public ReturnMessage spectateActiveGame(SystemUser user, int gameID)
	{
		ReturnMessage m = new ReturnMessage();
		TexasHoldemGame existingGame = dal.getGameById(gameID);
		if (existingGame != null)
		{
			Spectator spec = new Spectator(user.id);
			m = existingGame.joinSpectate(spec);
			if (m.success)
			{
				user.addSpectatingGame(spec);
			}
			return m;
		}
		else
			return new ReturnMessage(false, "Couldn't find the wanted game with the id:" + gameID.ToString() + ".");
	}

	public ReturnMessage joinActiveGame(SystemUser user, int gameID)
	{
		TexasHoldemGame existingGame = dal.getGameById(gameID);
		if (existingGame != null)
		{
			if (user.money >= existingGame.GamePreferences.BuyInPolicy)
			{
				Player p = new Player(user.id, existingGame.GamePreferences.BuyInPolicy, user.rank);
				return existingGame.joinGame(p);
			}
			else
				return new ReturnMessage(false, "Could not join the game because the user dont have enough money to join.");
		}
		else
			return new ReturnMessage(false, "Couldn't find the wanted game with the id:" + gameID.ToString() + ".");
	}

	public ReturnMessage leaveGame(Spectator spec, int gameID)
	{
		ReturnMessage m = new ReturnMessage();
		TexasHoldemGame existingGame = dal.getGameById(gameID);
		if (spec.GetType() == typeof(Player))
		{
			Player p = (Player)spec;
			existingGame.leaveGame(p);
			SystemUser user = dal.getUserById(spec.systemUserID);
			//TODO: what is the rank changing policy.
			user.money += p.Tokens;
			if (p.Tokens > existingGame.GamePreferences.BuyInPolicy)
				user.rank += 1;
			else
				user.rank -= 1;
			dal.editUser(user);
		}
		else
		{
			existingGame.leaveGame((Player)spec);
		}
		return m;
	}

	public ReturnMessage editUserProfile(int userId, string name, string password, string email, string avatar)
	{
		ReturnMessage m = new ReturnMessage();
		SystemUser user = dal.getUserById(userId);
		List<SystemUser> allUsers = dal.getAllUsers();
		if (name.Equals("") || password.Equals(""))
		{
			m.success = false;
			m.description = "Can't change to empty user name or password.";
			return m;
		}
		foreach (SystemUser u in allUsers) {
			if (u.name.Equals(name, StringComparison.OrdinalIgnoreCase) || u.email.Equals(email, StringComparison.OrdinalIgnoreCase)) //comparing two passwords including cases i.e AbC = aBc
			{
				m.success = false;
				m.description = "Username or email already exists.";
				return m;
			}
		}
		user.name = name;
		user.password = password;
		user.email = email;
		user.userImage = avatar;
		return m;
	}

	public List<TexasHoldemGame> findAllActiveAvailableGames()
	{
		List<TexasHoldemGame> ans = new List<TexasHoldemGame> { };
		foreach (TexasHoldemGame g in dal.getAllGames())
		{
			if (g.active && g.AvailableSeats > 0)
				ans.Add(g);
		}

		return ans;
	}

	public List<TexasHoldemGame> filterActiveGamesByPlayerName(string name)
	{
		List<TexasHoldemGame> ans = new List<TexasHoldemGame> { };
		foreach (TexasHoldemGame g in dal.getAllGames())
		{
			foreach (Player p in g.players)
			{
                if (p != null)
                {
                    if (dal.getUserById(p.systemUserID).name.Equals(name, StringComparison.OrdinalIgnoreCase))
                    {
                        ans.Add(g);
                        break;
                    }
                }
			}
		}

		return ans;
	}

    public ReturnMessage createGame(int gameCreatorId, GamePreferences pref)
    {
        if (dal.getUserById(gameCreatorId) == null)
        {
            return new ReturnMessage(false, "Game creator is Not a user.");
        }
        ReturnMessage m = checkGamePreferences(pref);
        if (m.success)
        {
            TexasHoldemGame game = new TexasHoldemGame(gameCreatorId, pref);
            dal.addGame(game);
        }
        return m;
    }

    private ReturnMessage checkGamePreferences(GamePreferences pref)
    {
        // Check buy in policy.
        if(pref.BuyInPolicy < 0)
        {
            return new ReturnMessage(
                false, 
                String.Format("Buy in policy is {0}. Should be equal or higher than zero.", pref.BuyInPolicy));
        }

        // Check max players.
        if(pref.MaxPlayers < 2 || pref.MaxPlayers > 9)
        {
            return new ReturnMessage(
                false,
                String.Format("Max players is {0}. Has to be greater than 1 and lesser than 9", pref.MaxPlayers));
        }

        // Check minimal bet.
        if(pref.MinimalBet <= 0)
        {
            return new ReturnMessage(
                false,
                String.Format("Minimal bet is {0}. Has to be greater or equal to zero.", pref.MinimalBet));
        }

        // Check minimal bet lower than chips count.
        if (pref.MinimalBet > pref.StartingChipsAmount)
        {
            return new ReturnMessage(
                false,
                String.Format("Minimal bet is {0}. Has to be lower or equal to starting chips amount {1}.", pref.MinimalBet, pref.StartingChipsAmount));
        }
        // Check min players.
        if (pref.MinPlayers > pref.MaxPlayers || pref.MinPlayers < 2)
        {
            return new ReturnMessage(
                false,
                String.Format("Min players are {0}. Has to be greater than 1 and lesser than max players.", pref.MinPlayers));
        }

        // Check starting chips amount.
        if(pref.StartingChipsAmount <= 0)
        {
            return new ReturnMessage(
                false,
                String.Format("Starting chips are {0}. Has to be greater than zero.", pref.StartingChipsAmount));
        }

        // Return all checks have passed.
        return new ReturnMessage(true, null);
    }

    public List<TexasHoldemGame> filterActiveGamesByPotSize(int potSize)
	{
		List<TexasHoldemGame> ans = new List<TexasHoldemGame> { };
		ans = dal.getAllGames();
		foreach (TexasHoldemGame g in ans)
		{
			if (g.pot > potSize)
			{
				ans.Remove(g);
			}
		}

		return ans;
	}

	public List<TexasHoldemGame> filterActiveGamesByGamePreferences(GamePreferences pref)
	{
		List<TexasHoldemGame> ans = new List<TexasHoldemGame> { };
		foreach (TexasHoldemGame g in dal.getAllGames())
		{
			if (g.GamePreferences.Equals(pref))
			{
				ans.Add(g);
			}
		}

		return ans;
	}

	public SystemUser getUserById(int userId)
	{
		return dal.getUserById(userId);
	}

	public SystemUser getUserByName(string name)
	{
		return dal.getUserByName(name);
	}

	public TexasHoldemGame getGameById(int gameId)
	{
		return dal.getGameById(gameId);
	}

	public ReturnMessage Login(string user, string password)
	{
		if(user == null || password == null || user.Equals("") || password.Equals(""))
			return new ReturnMessage(false, "all attributes must be filled.");

		SystemUser systemUser = dal.getUserByName(user);
		if (systemUser == null)
			return new ReturnMessage(false, "user does not exists\\incorrect password mismatched");

		if (systemUser.password.Equals(password))
			return dal.logUser(user);
		else
			return new ReturnMessage(false, "user does not exists\\incorrect password mismatched");

	}
	public ReturnMessage Register(string user, string password, string email, string userImage)
	{
		if (user == null || password == null || email == null || userImage == null || user.Equals("") || password.Equals("") || email.Equals("") || userImage.Equals(""))
			return new ReturnMessage(false, "all attributes must be filled.");

		SystemUser systemUser = dal.getUserByName(user);
		if (systemUser != null)
			return new ReturnMessage(false, "user name already taken");

		return dal.registerUser(new SystemUser(user, password, email, userImage, 0));
		
	}
	
    public League getLeagueById(Guid leagueId) {

        var leagues = dal.getAllLeagues();

        if (leagues.Exists(l => l.leagueId == leagueId)) {
            return leagues.Find(l => l.leagueId == leagueId);
        }

        else
        {
            return null;
        }
    }

    public League getLeagueByName(string leagueName)
    {
        var leagues = dal.getAllLeagues();

        if (leagues.Exists(l => l.leagueName == leagueName))
        {
            return leagues.Find(l => l.leagueName == leagueName);
        }

        else
        {
            return null;
        }
    }

    public ReturnMessage removeLeague(League league)
    {
        if(league!= null)
         return dal.removeLeague(league.leagueId);
        return new ReturnMessage(false, "leagueId is NULL");
    }

	public ReturnMessage Logout(string user)
	{
		if (user == null || user.Equals(""))
			return new ReturnMessage(false, "all attributes must be filled.");

		SystemUser systemUser = dal.getUserByName(user);
		if (systemUser == null)
			return new ReturnMessage(false, "User does not exists");

		if (systemUser.spectators.Count > 0)
			return new ReturnMessage(false, "you are active in some games, leave them and then log out.");

		return dal.logOutUser(user);
	}

    public ReturnMessage addLeague(int minRank, int maxRank, string leagueName)
    {
        var ranksMessage = isRanksLegal(minRank, maxRank);
        if (ranksMessage.success)
        {
            var leagues = dal.getAllLeagues();
            foreach (var l in leagues)
            {
                if (l.minRank == minRank && l.maxRank == maxRank)
                {
                    return new ReturnMessage(false, String.Format("Cannot create league. leagueId {0} has matching ranks, minRank {1}, maxRank {2}.", l.leagueId, minRank, maxRank));
                }

                if(l.leagueName == leagueName)
                {
                    return new ReturnMessage(false, String.Format("Cannot create league. leagueId {0} has matching name {1}.", l.leagueId, leagueName));
                }
            }
            
            return dal.addLeague(minRank, maxRank, leagueName);
        }
        else return ranksMessage;
    }

    public ReturnMessage setLeagueCriteria(int minRank, int maxRank, string leagueName, Guid leagueId, int userId)
    {
        var ranksMessage = isRanksLegal(minRank, maxRank);
        if (ranksMessage.success)
        {
            if (dal.getHighestUserId() != userId)
            {
                return new ReturnMessage(false, String.Format("Cannot set criteria. user {0} is not highest ranking in system.", userId));
            }

            if (dal.getAllLeagues().Any(l => (l.leagueName == leagueName && (l.leagueId != leagueId))))
            {
                return new ReturnMessage(false, String.Format("League name {0} already taken.", leagueName));
            }

            var league = dal.getAllLeagues().Where(l => l.leagueId == leagueId).FirstOrDefault();

            if (league == null)
            {
                return new ReturnMessage(false, String.Format("No such league with ID: {0}", leagueId));
            }

            return dal.setLeagueCriteria(minRank, maxRank, leagueName, leagueId, userId);
        }

        else return ranksMessage;
    }

    private ReturnMessage isRanksLegal(int minRank, int maxRank)
    {
        if (minRank < 0)
        {
            return new ReturnMessage(false, String.Format("Cannot create league with minRank {0}. invalid minRank.", minRank));
        }

        if (maxRank <= minRank)
        {
            return new ReturnMessage(false, String.Format("Cannot create league with minRank {0}, maxRank {1}. maxRank has to be bigger than minRank.", minRank, maxRank));
        }

        return new ReturnMessage(true, null);
    }
}