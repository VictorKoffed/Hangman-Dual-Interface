/*
 * FILE COMMENT: This file contains the core game mechanics and state management (GameStatus).
 * This logic was developed in collaboration with unit tests using the red, green, refactor
 * principle, with assistance from a large language model (AI) to ensure robust core logic.
 */

using Hangman.Core.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace Hangman.Core
{
    /// <summary>
    /// Responsible for the core game logic of Hangman.
    /// </summary>
    public sealed class Game
    {
        private string _secret = string.Empty;
        private readonly int _maxMistakes;
        private readonly HashSet<char> _used = new();

        public Game(int maxMistakes = 6)
        {
            if (maxMistakes <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxMistakes));

            _maxMistakes = maxMistakes;
        }

        public GameStatus Status { get; private set; } = GameStatus.InProgress;
        public int Mistakes { get; private set; } = 0;
        public int AttemptsLeft => _maxMistakes - Mistakes;
        public IReadOnlyCollection<char> UsedLetters => _used;
        public string Secret => _secret;

        // ───── EVENTS ──────────────────────────────────────────────
        public event EventHandler<char>? LetterGuessed;
        public event EventHandler<char>? WrongLetterGuessed;
        public event EventHandler<GameStatus>? GameEnded;
        // ──────────────────────────────────────────────────────────

        /// <summary>
        /// Starts a new game round.
        /// Resets the round state, clears previously used letters, and assigns the new secret word.
        /// Resetting all round-specific state is essential because a Game instance may be reused
        /// for multiple rounds.
        /// </summary>
        public void StartNew(string word)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Secret word cannot be empty", nameof(word));

            _secret = word.ToUpperInvariant();
            Status = GameStatus.InProgress;
            Mistakes = 0;
            _used.Clear();
        }

        /// <summary>
        /// Handles a letter guess.
        /// Returns true when the letter exists in the secret word; otherwise returns false.
        /// </summary>
        public bool Guess(char letter)
        {
            if (Status != GameStatus.InProgress) return false;

            var c = char.ToUpperInvariant(letter);

            // Duplicate guesses must not consume another attempt, since they do not represent
            // a new mistake and should therefore have no additional impact on the round state.
            if (_used.Contains(c))
                return _secret.Contains(c);

            _used.Add(c);

            // ───── Correct guess ───────────────────────────────
            if (_secret.Contains(c))
            {
                LetterGuessed?.Invoke(this, c);

                if (AllRevealed())
                {
                    Status = GameStatus.Won;
                    GameEnded?.Invoke(this, Status);
                }

                return true;
            }

            // ───── Incorrect guess ────────────────────────────
            Mistakes++;
            WrongLetterGuessed?.Invoke(this, c);

            if (Mistakes >= _maxMistakes)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }

            return false;
        }

        /// <summary>
        /// Returns the secret word in masked form so that only correctly guessed letters
        /// are revealed to the player.
        /// </summary>
        public string GetMaskedWord()
        {
            if (string.IsNullOrEmpty(_secret))
                return string.Empty;

            var chars = new char[_secret.Length];
            for (int i = 0; i < _secret.Length; i++)
            {
                var ch = _secret[i];
                chars[i] = _used.Contains(ch) ? ch : '_';
            }
            return new string(chars);
        }

        private bool AllRevealed()
        {
            var needed = new HashSet<char>(_secret);

            // Non-letter characters are excluded because punctuation and other symbols
            // do not represent guessable letters and therefore must not block a win.
            needed.RemoveWhere(ch => !char.IsLetter(ch));
            return needed.IsSubsetOf(_used);
        }

        /// <summary>
        /// Forces the game into the loss state.
        /// This is used when the player abandons a round or when the time limit expires,
        /// ensuring both situations follow the same game-end event flow as a normal loss.
        /// </summary>
        public void ForceLose()
        {
            if (Status == GameStatus.InProgress)
            {
                Status = GameStatus.Lost;
                GameEnded?.Invoke(this, Status);
            }
        }
    }
}
