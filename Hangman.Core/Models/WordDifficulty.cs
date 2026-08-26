using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Hangman.Core.Models
{
    /// <summary>
    /// Defines the standardized difficulty levels used when selecting words.
    /// Shared by all WordProviders to ensure consistent difficulty handling
    /// regardless of the underlying word source.
    /// </summary>
    public enum WordDifficulty
    {
        Easy,
        Medium,
        Hard
    }
}
