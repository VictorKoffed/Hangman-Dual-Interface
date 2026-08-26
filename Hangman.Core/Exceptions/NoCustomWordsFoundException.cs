using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Exceptions
{
    /// <summary>
    /// Thrown when a game round attempts to retrieve a word from a custom word list
    /// that contains no words matching the selected criteria.
    /// </summary>
    public class NoCustomWordsFoundException : InvalidOperationException
    {
        public WordDifficulty Difficulty { get; }
        public WordLanguage Language { get; }

        public NoCustomWordsFoundException(WordDifficulty difficulty, WordLanguage language)
            : base($"No custom words found for {difficulty} ({language}).") // Generic English message intended for logging
        {
            Difficulty = difficulty;
            Language = language;
        }
    }
}
