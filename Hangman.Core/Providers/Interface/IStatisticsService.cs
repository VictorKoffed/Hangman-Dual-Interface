using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Providers.Interface
{
    /// <summary>
    /// Defines the contract for managing high scores, including persistence and retrieval.
    /// </summary>
    public interface IStatisticsService
    {
        /// <summary>
        /// Saves a high score. The implementation determines whether the new result
        /// is better than an existing record and therefore worth persisting.
        /// </summary>
        /// <param name="newScore">The new high-score entry to evaluate and save.</param>
        Task SaveHighscoreAsync(HighscoreEntry newScore);

        /// <summary>
        /// Retrieves the best high scores for a specific difficulty,
        /// ordered by consecutive wins in descending order.
        /// </summary>
        /// <param name="difficulty">The difficulty level to filter by.</param>
        /// <returns>A list of <see cref="HighscoreEntry"/> records.</returns>
        Task<List<HighscoreEntry>> GetHighscoresAsync(WordDifficulty difficulty);

        /// <summary>
        /// Retrieves the top N results across all difficulty levels,
        /// allowing the application to present a combined global ranking.
        /// </summary>
        /// <param name="topN">The number of entries to return per difficulty level.</param>
        /// <returns>A flattened list containing the top results.</returns>
        Task<List<HighscoreEntry>> GetGlobalTopScoresAsync(int topN = 5);
    }
}
