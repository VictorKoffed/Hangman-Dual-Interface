using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

/*
  Interface for asynchronously retrieving words for the game.
  It is used for providers that may depend on network requests or file I/O.
*/

namespace Hangman.Core.Providers.Interface
{
    /// <summary>
    /// Defines a common contract for asynchronous word providers,
    /// allowing the game to switch between different word sources
    /// without coupling the game logic to a specific implementation.
    /// </summary>
    public interface IAsyncWordProvider
    {
        /// <summary>
        /// Retrieves a word asynchronously while allowing the operation
        /// to be cancelled by the caller when the current game flow no longer
        /// requires the pending word request.
        /// </summary>
        // The method now returns Task<string> to support asynchronous handling
        Task<string> GetWordAsync(CancellationToken ct = default);

        /// <summary>
        /// Gets the human-readable name of the difficulty or provider
        /// for display and identification purposes.
        /// </summary>
        string DifficultyName { get; }

        /// <summary>
        /// Gets the difficulty associated with the provider.
        /// Exposing the value through the interface keeps consumers independent
        /// of concrete provider implementations and avoids Reflection in the ViewModel.
        /// </summary>
        // Exposes the difficulty publicly to avoid Reflection in the ViewModel.
        WordDifficulty Difficulty { get; }
    }
}
