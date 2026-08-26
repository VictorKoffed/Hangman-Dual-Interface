/*
 * FILE COMMENT: This file contains all database access logic (CRUD operations)
 * for statistics and high scores using Entity Framework Core. The data abstraction
 * and asynchronous database operations were developed with assistance
 * from a large language model (AI).
 */

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;
using Hangman.Core.Providers.Interface;
using Microsoft.EntityFrameworkCore;

namespace Hangman.Core.Providers.Db
{
    /// <summary>
    /// Implements <see cref="IStatisticsService"/> by persisting high scores
    /// in a SQLite database using Entity Framework Core.
    /// </summary>
    public class SqliteHangmanService : IStatisticsService
    {
        private HangmanDbContext CreateContext() => new HangmanDbContext();

        /// <summary>
        /// Saves a high score only when it represents a meaningful result,
        /// and replaces an existing record only when the new result is better.
        /// This preserves the best achievement for each player and difficulty
        /// rather than allowing weaker results to overwrite it.
        /// </summary>
        public async Task SaveHighscoreAsync(HighscoreEntry newScore)
        {
            if (newScore.ConsecutiveWins <= 0) return;

            using (var context = CreateContext())
            {
                var existingScore = await context.Highscores.FirstOrDefaultAsync(s =>
                    s.PlayerName.ToLower() == newScore.PlayerName.ToLower() &&
                    s.Difficulty == newScore.Difficulty);

                bool saved = false;

                if (existingScore != null)
                {
                    if (newScore.ConsecutiveWins > existingScore.ConsecutiveWins)
                    {
                        existingScore.ConsecutiveWins = newScore.ConsecutiveWins;
                        newScore.Id = existingScore.Id;
                        context.Highscores.Update(existingScore);
                        await context.SaveChangesAsync();
                        saved = true;
                    }
                }
                else
                {
                    context.Highscores.Add(newScore);
                    await context.SaveChangesAsync();
                    saved = true;
                }

                if (saved)
                {
                    await PruneScoresAsync(context, newScore.Difficulty, 10);
                }
            }
        }

        /// <summary>
        /// Ensures that only the top <paramref name="topN"/> scores
        /// for a given difficulty remain in the database.
        /// Keeping the table bounded prevents obsolete ranking entries
        /// from accumulating when new high scores are recorded.
        /// </summary>
        private async Task PruneScoresAsync(HangmanDbContext context, WordDifficulty difficulty, int topN)
        {
            var scores = await context.Highscores
                .Where(s => s.Difficulty == difficulty)
                .OrderByDescending(s => s.ConsecutiveWins)
                .ToListAsync();

            if (scores.Count > topN)
            {
                var scoresToRemove = scores.Skip(topN);
                context.Highscores.RemoveRange(scoresToRemove);
                await context.SaveChangesAsync();
            }
        }

        /// <summary>
        /// Retrieves all high scores for the specified difficulty in ranking order.
        /// No-tracking queries are used because the returned entries are read-only
        /// and do not need to be tracked for later persistence.
        /// </summary>
        public async Task<List<HighscoreEntry>> GetHighscoresAsync(WordDifficulty difficulty)
        {
            using (var context = CreateContext())
            {
                return await context.Highscores
                    .AsNoTracking()
                    .Where(s => s.Difficulty == difficulty)
                    .OrderByDescending(s => s.ConsecutiveWins)
                    .ToListAsync();
            }
        }

        /// <summary>
        /// Retrieves the top scores for every difficulty so the application can
        /// present a combined global ranking while still preserving representation
        /// from each difficulty level.
        /// </summary>
        public async Task<List<HighscoreEntry>> GetGlobalTopScoresAsync(int topN = 5)
        {
            using (var context = CreateContext())
            {
                var topScores = new List<HighscoreEntry>();

                foreach (WordDifficulty difficulty in Enum.GetValues<WordDifficulty>())
                {
                    var topForDifficulty = await context.Highscores
                        .AsNoTracking()
                        .Where(s => s.Difficulty == difficulty)
                        .OrderByDescending(s => s.ConsecutiveWins)
                        .Take(topN)
                        .ToListAsync();

                    topScores.AddRange(topForDifficulty);
                }

                return topScores;
            }
        }
    }
}
