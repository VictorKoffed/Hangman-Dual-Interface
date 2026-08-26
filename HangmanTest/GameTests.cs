/*
 * FILE COMMENT: This file contains the Unit Tests for the core game engine (Hangman.Core/Game.cs).
 * The tests were developed in tandem with the core logic (following the Red-Green-Refactor TDD principle)
 * and have been designed to ensure high test coverage and robust game mechanics.
 * By keeping these tests isolated from the UI layer, we enforce the Clean Architecture boundary 
 * that the domain logic must remain framework-agnostic.
 */

using System;
using System.Collections.Generic;
using Hangman.Core;
using Hangman.Core.Models;
using Xunit;

namespace HangmanTest
{
    /// <summary>
    /// Test suite for verifying the state machine and core domain logic of the <see cref="Game"/> class.
    /// 
    /// Primary objectives:
    /// - Ensure predictable state initialization (StartNew).
    /// - Validate that player inputs (Guess) mutate state correctly without side effects.
    /// - Verify that terminal conditions (Win/Loss) lock the state machine and trigger appropriate domain events.
    /// - Confirm that data provided to the presentation layer (GetMaskedWord) is properly obfuscated to prevent data leaks.
    /// - Test edge cases (e.g., duplicate guesses, empty words, special characters) to guarantee robustness.
    /// 
    /// All tests follow the AAA (Arrange-Act-Assert) pattern for maximum readability.
    /// </summary>
    public class GameTests
    {
        /// <summary>
        /// Validates the initial state of the game engine.
        /// It is critical that counters and collections are fully reset so that reused game instances 
        /// do not bleed state from previous rounds.
        /// </summary>
        [Fact]
        public void StartNew_ShouldInitializeGameCorrectly()
        {
            // Arrange
            var game = new Game();

            // Act
            game.StartNew("Test");

            // Assert
            Assert.Equal(GameStatus.InProgress, game.Status);  // The state machine must unlock and enter 'InProgress' mode.
            Assert.Equal("TEST", game.Secret);                 // The secret word is normalized to uppercase to simplify case-insensitive comparisons later.
            Assert.Equal(0, game.Mistakes);                    // The mistake counter must start at exactly zero.
            Assert.Empty(game.UsedLetters);                    // The history of guesses must be completely cleared.
        }

        /// <summary>
        /// Verifies the positive progression path when a valid, correct guess is made.
        /// </summary>
        [Fact]
        public void Guess_CorrectLetter_ShouldReturnTrueAndNotIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var result = game.Guess('T');

            // Assert
            Assert.True(result);                     // The guess operation should report success.
            Assert.Equal(0, game.Mistakes);          // A correct guess must not penalize the player.
            Assert.Contains('T', game.UsedLetters);  // The letter must be recorded to prevent future duplicate guessing.
        }

        /// <summary>
        /// Verifies the negative progression path when an incorrect guess is made.
        /// </summary>
        [Fact]
        public void Guess_WrongLetter_ShouldReturnFalseAndIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var result = game.Guess('A');

            // Assert
            Assert.False(result);                    // The guess operation should report failure.
            Assert.Equal(1, game.Mistakes);          // The failure must increment the penalty counter.
            Assert.Contains('A', game.UsedLetters);  // The incorrect letter must still be logged to update the UI keyboard.
        }

        /// <summary>
        /// Ensures that the core domain handles string obfuscation rather than relying on the UI.
        /// This keeps business rules (like what constitutes a revealed letter) centralized.
        /// </summary>
        [Fact]
        public void GetMaskedWord_ShouldHideUnrevealedLetters_AndShowCorrectGuesses()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");
            game.Guess('T'); // Only 'T' is guessed, which appears twice.

            // Act
            var masked = game.GetMaskedWord();

            // Assert
            Assert.Equal("T__T", masked); // Validates that multiple occurrences are resolved simultaneously.
        }

        /// <summary>
        /// Validates a core business rule: Players should not be doubly penalized for memory lapses.
        /// Guessing a previously guessed incorrect letter should be a no-op (idempotent).
        /// </summary>
        [Fact]
        public void Guess_SameLetterTwice_ShouldNotIncreaseMistakes()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var first = game.Guess('A');  // First incorrect guess
            var second = game.Guess('A'); // Duplicate incorrect guess

            // Assert
            Assert.False(first);                 // The first attempt correctly fails.
            Assert.False(second);                // The second attempt also fails...
            Assert.Equal(1, game.Mistakes);      // ...but crucially, the penalty counter must NOT increment to 2.
            Assert.Contains('A', game.UsedLetters); 
                                                 
            // Extra sanity check: Ensure the internal collection inherently prevents duplicates 
            // (e.g., by utilizing a HashSet) to maintain optimal memory and lookup performance O(1).
            Assert.Equal(game.UsedLetters.Count, new HashSet<char>(game.UsedLetters).Count);
        }

        /// <summary>
        /// Verifies the Win-condition trigger and the subsequent domain event broadcast.
        /// </summary>
        [Fact]
        public void Guess_AllDistinctLettersGuessed_ShouldSetStatusWon_AndRaiseGameEnded()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");
            GameStatus? endedWith = null;
            int endedCount = 0;
            
            // Subscribing to the domain event to verify it fires exactly once with the correct payload.
            game.GameEnded += (_, status) => { endedWith = status; endedCount++; };

            // Act - Guess all unique letters (T, E, S). 'T' occurs twice but is resolved in one guess.
            Assert.True(game.Guess('T')); // Correct
            Assert.True(game.Guess('E')); // Correct
            Assert.True(game.Guess('S')); // Correct -> This action fulfills the win condition.

            // Assert
            Assert.Equal(GameStatus.Won, game.Status);          // State machine transitions to Won.
            Assert.Equal(1, endedCount);                        // The GameEnded event must fire exactly once to prevent duplicate UI dialogues.
            Assert.Equal(GameStatus.Won, endedWith);            // The event payload must match the new state.
            Assert.Equal("TEST", game.GetMaskedWord());         // The entire word must now be visible.
        }

        /// <summary>
        /// Verifies the Loss-condition trigger and ensures the state machine locks down against further mutations.
        /// </summary>
        [Fact]
        public void Guess_ReachingMaxMistakes_ShouldSetStatusLost_AndRaiseGameEnded()
        {
            // Arrange
            var game = new Game(maxMistakes: 2); // Lowered threshold to expedite the failure state in testing.
            game.StartNew("TEST");
            GameStatus? endedWith = null;
            int endedCount = 0;
            game.GameEnded += (_, status) => { endedWith = status; endedCount++; };

            // Act - Two incorrect guesses
            Assert.False(game.Guess('A')); // Incorrect
            Assert.False(game.Guess('B')); // Incorrect -> This action exhausts all attempts.

            // Assert
            Assert.Equal(GameStatus.Lost, game.Status);         // State machine transitions to Lost.
            Assert.Equal(0, game.AttemptsLeft);                 // Zero attempts should remain.
            Assert.Equal(1, endedCount);                        // The GameEnded event must fire exactly once.
            Assert.Equal(GameStatus.Lost, endedWith);           // The event payload must indicate a loss.

            // Extra validation: Once a terminal state is reached, the state machine must reject further mutations.
            var mistakesBefore = game.Mistakes;
            Assert.False(game.Guess('C'));                      // This late guess must be ignored.
            Assert.Equal(mistakesBefore, game.Mistakes);        // The state remains frozen.
        }

        /// <summary>
        /// Validates the fail-fast guard clause preventing the initialization of an unplayable game.
        /// </summary>
        [Fact]
        public void StartNew_WithEmptyWord_ShouldThrowArgumentException()
        {
            // Arrange
            var game = new Game();

            // Act + Assert
            // The domain model is responsible for protecting its own invariants. An empty word is inherently invalid.
            Assert.Throws<ArgumentException>(() => game.StartNew(""));
        }

        /// <summary>
        /// Confirms that the game engine is immutable once a terminal state is reached.
        /// </summary>
        [Fact]
        public void Guess_AfterGameEnded_ShouldNotChangeState()
        {
            // Arrange
            var game = new Game(maxMistakes: 1);
            game.StartNew("TEST");
            game.Guess('A'); // Single mistake exhausts the 1 allowed attempt -> Game Lost.

            var mistakesBefore = game.Mistakes;

            // Act - Attempt to interact with a finalized game.
            var result = game.Guess('T');

            // Assert
            Assert.False(result);                        // The operation is rejected.
            Assert.Equal(mistakesBefore, game.Mistakes); // Internal state remains unaltered.
            Assert.Equal(GameStatus.Lost, game.Status);  // Terminal status is preserved.
        }

        /// <summary>
        /// Ensures the starting condition provides zero information to the player.
        /// </summary>
        [Fact]
        public void GetMaskedWord_ImmediatelyAfterStart_ShouldReturnOnlyUnderscores()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");

            // Act
            var masked = game.GetMaskedWord();

            // Assert
            // The length of the obfuscated string must match the secret, but expose no characters.
            Assert.Equal("____", masked);
        }

        /// <summary>
        /// Verifies a crucial business rule: Special characters (like hyphens or spaces) are provided "for free".
        /// The player is only required to guess valid alphabet characters to win.
        /// </summary>
        [Fact]
        public void Guess_AllLettersInWordWithDash_ShouldStillWin()
        {
            // Arrange
            var game = new Game();
            game.StartNew("A-B");

            GameStatus? ended = null;
            game.GameEnded += (_, status) => ended = status;

            // Act
            // Guessing the only two alphabetic characters in the string.
            game.Guess('A');
            game.Guess('B');

            // Assert
            // The engine must dynamically ignore the '-' when calculating if all letters have been discovered.
            Assert.Equal(GameStatus.Won, game.Status);
            Assert.Equal(GameStatus.Won, ended);
        }

        /// <summary>
        /// Ensures domain events are idempotent. Firing them multiple times for the same input 
        /// could cause UI glitches (like playing an animation or sound effect twice).
        /// </summary>
        [Fact]
        public void Guess_SameCorrectLetterTwice_ShouldNotRaiseEventsTwice()
        {
            // Arrange
            var game = new Game();
            game.StartNew("TEST");
            int rightCount = 0;
            
            // Subscribing to monitor duplicate firings.
            game.LetterGuessed += (_, _) => rightCount++;

            // Act
            // First correct guess.
            Assert.True(game.Guess('T'));

            // Second guess - same letter but different casing to verify normalization handles this.
            Assert.True(game.Guess('t'));

            // Assert
            Assert.Equal(1, rightCount);      // The event should only fire the first time the letter is discovered.
            Assert.Equal(0, game.Mistakes);   // No penalties applied.
        }

        /// <summary>
        /// Verifies that an incorrect guess correctly applies the penalty and fires the specific failure event exactly once,
        /// even if the user spams the same wrong key.
        /// </summary>
        [Fact]
        public void Guess_WrongLetter_RaisesEventOnce_And_DecreasesAttemptsLeft()
        {
            // Arrange
            var game = new Game(maxMistakes: 3);
            game.StartNew("TEST");
            int wrongCount = 0;
            game.WrongLetterGuessed += (_, _) => wrongCount++;

            var before = game.AttemptsLeft;

            // Act
            // First incorrect guess.
            Assert.False(game.Guess('A'));

            // Duplicate incorrect guess.
            Assert.False(game.Guess('A'));

            // Assert
            Assert.Equal(1, wrongCount);                 // Event fired exactly once.
            Assert.Equal(before - 1, game.AttemptsLeft); // Attempts reduced by exactly 1, ignoring the duplicate guess.
            Assert.Equal(1, game.Mistakes);              // Corresponds to the single registered penalty.
        }

        /// <summary>
        /// Confirms that the system is forgiving with user input and handles casing variations seamlessly.
        /// This centralizes normalization logic so the UI doesn't have to manually format inputs.
        /// </summary>
        [Fact]
        public void Guess_IsCaseInsensitive()
        {
            // Arrange
            var game = new Game();
            // The secret word utilizes mixed casing.
            game.StartNew("Test");

            // Act
            // The guesses are also mixed casing.
            var upper = game.Guess('T'); 
            var lower = game.Guess('e'); 

            // Assert
            // Both guesses must resolve correctly regardless of original casing.
            Assert.True(upper);
            Assert.True(lower);

            // The domain should internally store the history uniformly (as uppercase) to guarantee consistency.
            Assert.Contains('T', game.UsedLetters);
            Assert.Contains('E', game.UsedLetters);

            // The masked output should preserve the uppercase normalization for a clean UI presentation.
            Assert.Equal("TE_T", game.GetMaskedWord());
        }
    }
}
