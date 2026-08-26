using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Represents a player's highscore together with the difficulty level
    /// under which the consecutive wins were achieved.
    /// </summary>
    public class HighscoreEntry
    {
        public int Id { get; set; }

        public required string PlayerName { get; set; }
        public required int ConsecutiveWins { get; set; }
        public required WordDifficulty Difficulty { get; set; }

        [SetsRequiredMembers]
        public HighscoreEntry()
        {
            // Safe defaults are required for persistence and object materialization
            // scenarios where the actual values are populated after construction.
            PlayerName = string.Empty;
            ConsecutiveWins = 0;
            Difficulty = default(WordDifficulty);
        }

        /// <summary>
        /// Creates a highscore entry from the values recorded for a completed
        /// single-player game session.
        /// </summary>
        public HighscoreEntry(string playerName, int consecutiveWins, WordDifficulty difficulty)
        {
            PlayerName = playerName;
            ConsecutiveWins = consecutiveWins;
            Difficulty = difficulty;
        }
    }
}
