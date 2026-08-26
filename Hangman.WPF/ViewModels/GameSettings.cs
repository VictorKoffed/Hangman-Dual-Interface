using Hangman.Core.Models;

namespace Hangman.WPF.ViewModels
{
    /// <summary>
    /// Defines the available game modes so the application can select the
    /// appropriate game flow without coupling the UI to a specific implementation.
    /// </summary>
    public enum GameMode
    {
        SinglePlayer,
        Tournament
    }

    /// <summary>
    /// Represents the configuration required to start a game.
    /// Keeping these settings together allows the selected game options to be
    /// passed between the menu and game flow as a single state object.
    /// </summary>
    public class GameSettings
    {
        public string PlayerName { get; set; } = "Player 1";
        public string PlayerName2 { get; set; } = "Player 2";
        public WordDifficulty Difficulty { get; set; } = WordDifficulty.Medium;
        public WordSource Source { get; set; } = WordSource.Api;
    }

    /// <summary>
    /// Defines the available word sources so the game can switch between
    /// external, local, and language-specific custom word collections.
    /// </summary>
    public enum WordSource
    {
        Api,
        Local,
        CustomSwedish,
        CustomEnglish
    }
}
