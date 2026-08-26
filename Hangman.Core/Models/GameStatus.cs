/*
  Represents the possible states of a game:
  - InProgress = the game is still active
  - Won = the player won
  - Lost = the player lost
  - Draw = neither player won
*/

namespace Hangman.Core.Models
{
    /// <summary>
    /// Defines the states used to represent the outcome and lifecycle of a game.
    /// </summary>
    public enum GameStatus
    {
        InProgress,
        Won,
        Lost,
        Draw
    }
}
