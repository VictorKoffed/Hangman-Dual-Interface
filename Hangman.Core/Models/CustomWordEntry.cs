using Hangman.Core.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Represents a custom word together with the difficulty and language
    /// required to determine when it is eligible for a game round.
    /// </summary>
    public class CustomWordEntry
    {
        public int Id { get; set; }

        public required string Word { get; set; }

        public required WordDifficulty Difficulty { get; set; }

        public required WordLanguage Language { get; set; }

        [SetsRequiredMembers]
        public CustomWordEntry()
        {
            // The parameterless constructor provides safe defaults for persistence
            // and object materialization scenarios where values are populated later.
            Word = string.Empty;
            Difficulty = default;
            Language = default;
        }

        public CustomWordEntry(string word, WordDifficulty difficulty, WordLanguage language)
        {
            // Normalizing the word at the model boundary keeps comparisons and
            // gameplay consistent regardless of how the word was originally entered.
            Word = word.ToUpperInvariant();
            Difficulty = difficulty;
            Language = language;
        }
    }
}
