/*
 * FILE COMMENT: This file handles the complex state management for a two-player game,
 * including hiding the secret word and tracking the game progression. This logic
 * was structured with assistance from a large language model (AI).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Providers.Interface;
using Hangman.Core.Models;

namespace Hangman.Core
{
    /// <summary>
    /// Represents a player in two-player mode.
    /// </summary>
    public class Player
    {
        public string Name { get; }
        public int Lives { get; set; }
        public int Wins { get; set; }

        public Player(string name, int initialLives)
        {
            Name = name;
            Lives = initialLives;
            Wins = 0;
        }
    }

    /// <summary>
    /// Manages the tournament between two players, including lives, round progression,
    /// and victory conditions.
    /// </summary>
    public class TwoPlayerGame
    {
        public const int MaxLives = 3;
        private readonly IAsyncWordProvider _wordProvider;
        private readonly Random _rng = new Random();

        public Player Player1 { get; }
        public Player Player2 { get; }
        public Player? CurrentGuesser { get; private set; }
        public Game? CurrentRound { get; private set; }

        /// <summary>
        /// Gets the name of the player whose turn it is to guess.
        /// </summary>
        public string CurrentPlayerName => CurrentGuesser?.Name ?? "Inget";

        /// <summary>
        /// Gets the current status of the tournament.
        /// </summary>
        public GameStatus TournamentStatus { get; private set; } = GameStatus.InProgress;

        /// <summary>
        /// Initializes a new two-player tournament.
        /// The starting player is randomized so neither player consistently receives
        /// the first-turn advantage across separate tournaments.
        /// </summary>
        public TwoPlayerGame(string p1Name, string p2Name, IAsyncWordProvider wordProvider)
        {
            Player1 = new Player(p1Name, MaxLives);
            Player2 = new Player(p2Name, MaxLives);
            _wordProvider = wordProvider;

            CurrentGuesser = (_rng.Next(2) == 0) ? Player1 : Player2;
        }

        /// <summary>
        /// Starts a new game round by retrieving a word from the configured provider.
        /// Returns the secret word, or null when the tournament has already reached
        /// a terminal state.
        /// </summary>
        /// <returns>The secret word, or null if the tournament has ended.</returns>
        public async Task<string?> StartNewRoundAsync()
        {
            Player opponent = (CurrentGuesser == Player1) ? Player2 : Player1;

            if (opponent.Lives <= 0)
            {
                // When the opponent has no remaining lives, the number of wins determines
                // the winner; equal win counts result in a draw rather than another round.
                if (Player1.Wins != Player2.Wins)
                {
                    TournamentStatus = GameStatus.Lost;
                }
                else
                {
                    TournamentStatus = GameStatus.Draw;
                }
            }

            if (TournamentStatus != GameStatus.InProgress)
            {
                return null; // NEW: Returns null instead of throwing an exception
            }

            string secret = await _wordProvider.GetWordAsync();

            CurrentRound = new Game(6);
            CurrentRound.StartNew(secret);

            return secret;
        }

        /// <summary>
        /// Handles the result of the most recently completed round and updates
        /// the players' lives and turn order.
        /// </summary>
        /// <param name="roundResult">The result of the round (Won or Lost).</param>
        public void HandleRoundEnd(GameStatus roundResult)
        {
            if (CurrentGuesser == null)
            {
                throw new InvalidOperationException("Kan inte hantera runda. Ingen aktiv gissare.");
            }

            Player guessingPlayer = CurrentGuesser;

            // 1. Handle Lives and Wins
            if (roundResult == GameStatus.Won)
            {
                guessingPlayer.Wins++;
                guessingPlayer.Lives = MaxLives;
            }
            else // Lost
            {
                guessingPlayer.Lives--;
            }

            // 2. Switch Guesser
            CurrentGuesser = (CurrentGuesser == Player1) ? Player2 : Player1;
        }

        /// <summary>
        /// Returns the player who won the tournament, or null while the tournament
        /// is still in progress or ended in a draw.
        /// </summary>
        public Player? GetWinner()
        {
            if (Player1.Lives > 0 && Player2.Lives <= 0)
            {
                return Player1;
            }

            if (Player2.Lives > 0 && Player1.Lives <= 0)
            {
                return Player2;
            }

            // When both players have no lives remaining, wins are used as the tie-breaker.
            // Equal win counts intentionally produce no winner because the tournament is a draw.
            if (Player1.Lives <= 0 && Player2.Lives <= 0)
            {
                if (Player1.Wins > Player2.Wins)
                {
                    return Player1;
                }
                if (Player2.Wins > Player1.Wins)
                {
                    return Player2;
                }
            }

            return null;
        }
    }
}
