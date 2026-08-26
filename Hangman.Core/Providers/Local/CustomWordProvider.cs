using Hangman.Core.Exceptions;
using Hangman.Core.Models;
using Hangman.Core.Providers.Db;
using Hangman.Core.Providers.Interface;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Providers.Local
{
    /// <summary>
    /// Retrieves and stores custom words in the SQLite database.
    /// </summary>
    public sealed class CustomWordProvider : IAsyncWordProvider
    {

        private readonly Random _rng = new Random();
        private readonly WordDifficulty _difficulty;
        private readonly WordLanguage _language;

        public CustomWordProvider(WordDifficulty difficulty, WordLanguage language)
        {
            _difficulty = difficulty;
            _language = language;
        }

        public string DifficultyName => $"Anpassad Ordlista ({_language} - {_difficulty})";

        /// <summary>
        /// Gets the difficulty configured for this provider so consumers can use
        /// custom word sources through the same provider abstraction as other word sources.
        /// </summary>
        // IMPLEMENTATION: Now public and exposed through the interface
        public WordDifficulty Difficulty => _difficulty;

        /// <summary>
        /// Saves a new word to the custom word list in the database.
        /// </summary>
        public async Task AddWordAsync(string word, WordDifficulty difficulty, WordLanguage language)
        {
            if (string.IsNullOrWhiteSpace(word))
                throw new ArgumentException("Word cannot be null or whitespace.", nameof(word));

            var newEntry = new CustomWordEntry(word, difficulty, language)
            {
                Word = word,
                Difficulty = difficulty,
                Language = language
            };

            using (var context = new HangmanDbContext())
            {
                // Enforces the business rule that the same word may only exist once
                // for a specific combination of word, difficulty, and language.
                if (await context.CustomWords.AnyAsync(e =>
                    e.Word == newEntry.Word &&
                    e.Difficulty == newEntry.Difficulty &&
                    e.Language == newEntry.Language))
                {
                    throw new WordAlreadyExistsException(newEntry.Word, newEntry.Difficulty, newEntry.Language);
                }

                context.CustomWords.Add(newEntry);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves a random word matching the provider's configured difficulty and language.
        /// A random selection ensures that repeated game rounds do not always use the same word
        /// when a custom word list contains multiple valid entries.
        /// </summary>
        public async Task<string> GetWordAsync(CancellationToken ct = default)
        {
            List<string> availableWords;

            using (var context = new HangmanDbContext())
            {
                availableWords = await context.CustomWords
                    .AsNoTracking()
                    .Where(w => w.Difficulty == _difficulty && w.Language == _language)
                    .Select(w => w.Word)
                    .ToListAsync(ct);
            }

            if (!availableWords.Any())
            {
                throw new NoCustomWordsFoundException(_difficulty, _language);
            }

            int index = _rng.Next(availableWords.Count);
            return availableWords[index];
        }
    }
}
