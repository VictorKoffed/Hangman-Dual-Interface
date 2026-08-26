using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Exceptions
{
    /// <summary>
    /// Thrown when an attempt is made to add a word that already exists
    /// in the database with the same difficulty and language.
    /// </summary>
    public class WordAlreadyExistsException : InvalidOperationException
    {
        public string Word { get; }
        public WordDifficulty Difficulty { get; }
        public WordLanguage Language { get; }

        public WordAlreadyExistsException(string word, WordDifficulty difficulty, WordLanguage language)
            : base($"Word '{word}' already exists for {difficulty} ({language}).") // Generic English message intended for logging
        {
            Word = word;
            Difficulty = difficulty;
            Language = language;
        }
    }
}
