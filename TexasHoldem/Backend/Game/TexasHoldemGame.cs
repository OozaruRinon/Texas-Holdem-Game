﻿using System.Collections.Generic;
using Backend.User;
using System;

namespace Backend.Game
{
    public class TexasHoldemGame : Messages.Notification
    {
        private int availableSeats;
        public int currentDealer { get; set; }
        public int currentBig { get; set; }
        public int currentSmall { get; set; }
        public GamePreferences GamePreferences { get; }
        public Deck deck { get; }
        public Player[] players { get; set; }
        public List<Spectator> spectators;
        private int gameCreatorUserId;
        public int id { get; set; }
        public int pot { get; set; }
        public int tempPot { get; set; }
        public int currentBet { get; set; }
        public bool active { get; set; }
        public int currentBlindBet { get; set; }
        public enum BetAction { fold, bet, call, check, raise }
        public List<Card> flop { get; set; }
        public Card turn { get; set; }
        public Card river { get; set; }
        public enum HandsRanks { HighCard, Pair, TwoPairs, ThreeOfAKind, Straight, Flush, FullHouse, FourOfAKind, StraightFlush, RoyalFlush }


        public TexasHoldemGame(int gameCreatorUserId, GamePreferences gamePreferences)
        {
            this.gameCreatorUserId = gameCreatorUserId;
            this.GamePreferences = gamePreferences;
            pot = 0;
            active = true;
            deck = new Deck();
            spectators = new List<Spectator>();
            players = new Player[GamePreferences.MaxPlayers];
            availableSeats = GamePreferences.MaxPlayers - 1;
            players[0] = new Player(gameCreatorUserId, gamePreferences.StartingChipsAmount, 0);
            for (int i = 1; i < GamePreferences.MaxPlayers; i++)
            {
                players[i] = null;
            }

            flop = new List<Card>();

            currentDealer = 0;
        }

        public virtual ReturnMessage joinGame(Player p)
        {
            if (AvailableSeats == 0)
                return new ReturnMessage(false, "There are no available seats.");

            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].systemUserID == p.systemUserID)
                    return new ReturnMessage(false, "The player is already taking part in the wanted game.");
            }

            if (spectators != null)
            {
                foreach (Spectator spec in spectators)
                    if (spec.systemUserID == p.systemUserID)
                        return new ReturnMessage(false, "Couldn't join the game because the user is already spectating the game.");
            }
            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] == null)
                {
                    players[i] = p;
                    break;
                }
            }
            return new ReturnMessage(true, "");
        }

        public int AvailableSeats
        {
            get
            {
                int ans = 0;
                for (int i = 0; i < GamePreferences.MaxPlayers; i++)
                {
                    if (players[i] == null)
                        ans++;
                }
                return ans;
            }
        }

        public ReturnMessage joinSpectate(Spectator s)
        {
            if (!GamePreferences.IsSpectatingAllowed)
                return new ReturnMessage(false, "Couldn't spectate the game because the game preferences is not alowing.");

            foreach (Player p in players)
                if (p != null)
                {
                    if (p.systemUserID == s.systemUserID)
                        return new ReturnMessage(false, "Couldn't spectate the game because the user is already playing the game.");
                }
            foreach (Spectator spec in spectators)
                if (spec.systemUserID == s.systemUserID)
                    return new ReturnMessage(false, "Couldn't spectate the game because the user is already spectating the game.");

            spectators.Add(s);
            return new ReturnMessage(true, "");
        }

        public void leaveGame(Player p)
        {
            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].id == p.id)
                {
                    players[i] = null;
                    break;
                }
            }
        }

        public void leaveGame(Spectator spec)
        {
            spectators.Remove(spec);
        }

        private void playGame()
        {
            deck.Shuffle();
            dealCards();
            currentSmall = setSmallBlind();
            currentBig = setBigBlind();
            betBlinds();

            //players sets their bets
            for (int i = nextToSeat(currentBig); i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].playerState == Player.PlayerState.in_round)
                {
                    chooseBetAction(players[i], BetAction.check, 0);
                }
            }

            pot = tempPot;

            //flop
            for (int i = 0; i < 3; i++)
            {
                flop.Add(deck.Top());
            }

            //players sets their bets
            for (int i = nextToSeat(currentDealer); i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].playerState == Player.PlayerState.in_round)
                {
                    chooseBetAction(players[i], BetAction.check, 0);
                }
            }

            pot = tempPot;

            turn = deck.Top();

            //players sets their bets
            for (int i = nextToSeat(currentDealer); i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].playerState == Player.PlayerState.in_round)
                {
                    chooseBetAction(players[i], BetAction.check, 0);
                }
            }

            pot = tempPot;

            river = deck.Top();

            //players sets their bets
            for (int i = nextToSeat(currentDealer); i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null && players[i].playerState == Player.PlayerState.in_round)
                {
                    chooseBetAction(players[i], BetAction.check, 0);
                }
            }

            pot = tempPot;


        }

        public void setInitialState()
        {
            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[i] != null)
                    players[i].playerState = Player.PlayerState.in_round;
            }
        }


        public void dealCards()
        {
            int index = nextToSeat(currentDealer);

            //deal first card
            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[index] != null)
                {
                    Console.Out.WriteLine(" ggg    " + players[index].id);
                    if (players[index].playerCards.Count == 2)
                    {
                        players[index].playerCards.RemoveAt(0);
                        players[index].playerCards.RemoveAt(1);
                    }
                    players[index].playerCards.Add(deck.Top());
                }
                index = (index + 1) % GamePreferences.MaxPlayers;
            }

            index = nextToSeat(currentDealer);
            //deal second card
            for (int i = 0; i < GamePreferences.MaxPlayers; i++)
            {
                if (players[index] != null)
                {
                    players[index].playerCards.Add(deck.Top());
                }
                index = (index + 1) % GamePreferences.MaxPlayers;
            }
        }

        public int setSmallBlind()
        {
            int i = currentDealer;
            int j = (currentDealer + 1) % GamePreferences.MaxPlayers;
            while (i != j)
            {
                if (players[j] != null && players[j].playerState == Player.PlayerState.in_round)
                    return j;
                j = (j + 1) % GamePreferences.MaxPlayers;
            }
            return -1;
        }

        public int setBigBlind()
        {
            int i = currentDealer;
            int j = (currentDealer + 1) % GamePreferences.MaxPlayers;
            while (i != j)
            {
                if (players[j] != null && players[j].playerState == Player.PlayerState.in_round)
                    break;
                j = (j + 1) % GamePreferences.MaxPlayers;
            }
            j = (j + 1) % GamePreferences.MaxPlayers;
            while (i != j)
            {
                if (players[j] != null && players[j].playerState == Player.PlayerState.in_round)
                    return j;
                j = (j + 1) % GamePreferences.MaxPlayers;
            }
            return -1;
        }

        public void betBlinds()
        {
            players[currentSmall].Tokens -= currentBlindBet / 2;
            players[currentBig].Tokens -= currentBlindBet;
            tempPot += (currentBlindBet + (currentBlindBet / 2));
        }

        public void bet(Player p, int amount)
        {
            p.Tokens -= amount;
            tempPot += amount;
            currentBet = amount;
        }

        public void call(Player p)
        {
            p.Tokens -= currentBet;
            tempPot += currentBet;
        }

        public void fold(Player p)
        {
            p.playerState = Player.PlayerState.folded;
        }

        public void check(Player p)
        {
        }

        public void raise(Player p, int amount)
        {
            p.Tokens -= (amount + currentBet);
            tempPot += (amount + currentBet);
        }

        public void chooseBetAction(Player p, BetAction betAction, int amount)
        {
            switch (betAction)
            {
                case BetAction.bet:
                    bet(p, amount);
                    break;
                case BetAction.call:
                    call(p);
                    break;
                case BetAction.check:
                    check(p);
                    break;
                case BetAction.fold:
                    fold(p);
                    break;
                case BetAction.raise:
                    raise(p, amount);
                    break;
            }
        }

        public int nextToSeat(int seat)
        {
            int i = seat;
            int j = (i + 1) % GamePreferences.MaxPlayers;
            while (i != j)
            {
                if (players[j] != null && players[j].playerState == Player.PlayerState.in_round)
                    return j;
                j = (j + 1) % GamePreferences.MaxPlayers;
            }
            return -1;
        }


        public HandsRanks checkHandRank(Player p)
        {
            List<Card> fullHand = new List<Card>();
            for (int i = 0; i < 3; i++)
            {
                fullHand.Add(flop[0]);
                flop.RemoveAt(0);
            }

            for (int i = 0; i < 2; i++)
            {
                fullHand.Add(p.playerCards[0]);
            }

            fullHand.Add(turn);
            fullHand.Add(river);

            fullHand.Sort();

            if (checkRoyalFlush(fullHand))
                return HandsRanks.RoyalFlush;
            if (checkStraightFlush(fullHand) != -1)
                return HandsRanks.StraightFlush;



            for (int i = 0; i < fullHand.Count; i++)
            {
                Console.Out.Write(fullHand[i].Value + "  ");
                Console.Out.Write(fullHand[i].Type.ToString() + "  ");
                Console.Out.WriteLine();
            }


            return HandsRanks.Flush;
        }



        public bool checkRoyalFlush(List<Card> fullHand)
        {
            int[] counters = new int[4];
            for (int i = 0; i < 4; i++)
                counters[i] = 0;

            for (int i = 0; i < 7; i++)
                if (fullHand[i].Value == 1 ||
                    fullHand[i].Value == 10 ||
                    fullHand[i].Value == 11 ||
                    fullHand[i].Value == 12 ||
                    fullHand[i].Value == 13)

                    counters[(int)fullHand[i].Type]++;

            for (int i = 0; i < 4; i++)
                if (counters[i] == 5)
                    return true;
            return false;
        }

        public int checkStraightFlush(List<Card> fullHand)
        {
            int highestCardValue = 0;
            int[] valueBits = new int[4];
            for (int i = 0; i < 4; i++)
                valueBits[i] = 0;

            for (int i = 0; i < 7; i++)
                valueBits[(int)fullHand[i].Type] += (int)Math.Pow(2, fullHand[i].Value - 1);

            int bitCounter = 0;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 13; j++)
                {
                    if (valueBits[i] % 2 == 1)
                    {
                        bitCounter++;
                        valueBits[i] = valueBits[i] / 2;
                        Console.Out.WriteLine(bitCounter + "    first  ");

                        if (bitCounter == 5)
                        {
                            highestCardValue = j + 1;
                        }
                    }
                    else
                    {
                        bitCounter = 0;
                        valueBits[i] = valueBits[i] / 2;
                    }
                    if (bitCounter == 5)
                    {
                        return highestCardValue;
                    }
                }
            return -1;
        }

        public int checkFourOfAKind(List<Card> fullHand)
        {
            int cardValueOfFourOfAKind = 0;
            int[] fourOfAKindCounter = new int[13];
            for (int i = 0; i < 13; i++)
                fourOfAKindCounter[i] = 0;
            for (int i = 0; i < 7; i++)
            {
                fourOfAKindCounter[fullHand[i].Value - 1]++;
                if (fourOfAKindCounter[fullHand[i].Value - 1] == 4)
                    cardValueOfFourOfAKind = fullHand[i].Value;
            }

            for (int i = 0; i < 13; i++)
            {
                if (fourOfAKindCounter[i] == 4)
                {
                    return cardValueOfFourOfAKind;
                }
            }
            return -1;
        }

        public int checkFullHouse(List<Card> fullHand, int whatToReturn)
        {
            int[] threeOfAKindCounter = new int[13];
            int[] twoOfAKindCounter = new int[13];

            bool threeOfAKind = false;
            bool twoOfAKind = false;

            int threeOfAKindCardValue = 0;
            int twoOfAKindCardValue = 0;

            for (int i = 0; i < 13; i++)
            {
                threeOfAKindCounter[i] = 0;
                twoOfAKindCounter[i] = 0;
            }

            for (int i = 0; i < 7; i++)
            {
                threeOfAKindCounter[fullHand[i].Value - 1]++;
                twoOfAKindCounter[fullHand[i].Value - 1]++;

                if (threeOfAKindCounter[fullHand[i].Value - 1] == 3)
                    threeOfAKindCardValue = fullHand[i].Value;
                if (twoOfAKindCounter[fullHand[i].Value - 1] == 2)
                    twoOfAKindCardValue = fullHand[i].Value;
            }

            for (int i = 0; i < 13; i++)
            {
                if (threeOfAKindCounter[i] == 3)
                    threeOfAKind = true;
                if (twoOfAKindCounter[i] == 2)
                    twoOfAKind = true;
            }

            if (twoOfAKind && threeOfAKind)
                switch (whatToReturn)
                {
                    case 2:
                        return twoOfAKindCardValue;
                    case 3:
                        return threeOfAKindCardValue;
                }
            return -1;

        }


        public int checkStraight(List<Card> fullHand)
        {
            int valueCounter = 0;
            for (int i = 0; i < 7; i++)
                if (fullHand[i].Value == 1 ||
                    fullHand[i].Value == 10 ||
                    fullHand[i].Value == 11 ||
                    fullHand[i].Value == 12 ||
                    fullHand[i].Value == 13)
                    valueCounter++;
            if (valueCounter == 5)
                return 1;

            int valueBits = 0;

            for (int i = 0; i < 7; i++)
                valueBits += (int)Math.Pow(2, fullHand[i].Value - 1);

            int bitCounter = 0;
            int highestCardValue = 0;

            for (int j = 0; j < 13; j++)
            {
                if (valueBits % 2 == 1)
                {
                    bitCounter++;
                    valueBits = valueBits / 2;
                    if (bitCounter == 5)
                        highestCardValue = j + 1;
                }
                else
                {
                    bitCounter = 0;
                    valueBits = valueBits / 2;
                }
                if (bitCounter == 5)
                    return highestCardValue;
            }
            return -1;
        }
    }
}