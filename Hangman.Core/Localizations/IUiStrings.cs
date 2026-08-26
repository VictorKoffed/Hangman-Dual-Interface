using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Hangman.Core.Models;

namespace Hangman.Core.Localizations
{
    /// <summary>
    /// Defines the contract for all UI strings used by the application.
    /// Implementations provide language-specific text so presentation logic
    /// does not need to contain hard-coded user-facing strings.
    /// </summary>
    public interface IUiStrings
    {
        // Welcome screen
        string WelcomeTitleArt { get; }
        string WelcomeMessage { get; }
        string WelcomePressAnyKey { get; }

        // Main menu
        string MainMenuTitleArt { get; }
        string MainMenuGallowsArt { get; }
        string MainMenuTitle { get; }
        string MenuPlaySingle { get; }
        string MenuPlayTournament { get; }
        string MenuShowHighscores { get; }
        string MenuAddWord { get; }
        string MenuHelp { get; }
        string MenuQuit { get; }
        string MenuChoicePrompt { get; }
        string FeedbackThanksForPlaying { get; }

        // Word source selection
        string SelectWordSourceTitle { get; }
        string SelectWordSourceApi { get; }
        string SelectWordSourceLocal { get; }
        string SelectWordSourceCustomSwedish { get; }
        string SelectWordSourceCustomEnglish { get; }
        string SelectWordSourcePrompt { get; }
        string FeedbackWordSourceApi { get; }
        string FeedbackWordSourceLocal { get; }
        string FeedbackWordSourceCustomSwedish { get; }
        string FeedbackWordSourceCustomEnglish { get; }

        // Difficulty selection
        string SelectDifficultyTitle(string source);
        string SelectDifficultyEasy { get; }
        string SelectDifficultyMedium { get; }
        string SelectDifficultyHard { get; }
        string SelectDifficultyPrompt { get; }
        string FeedbackDifficultyEasy { get; }
        string FeedbackDifficultyMedium { get; }
        string FeedbackDifficultyHard { get; }

        // Shared UI
        string CommonPressEscapeToCancel { get; }
        string CommonPressAnyKeyToContinue { get; }
        string CommonFeedbackCancelling { get; }
        string CommonErrorDatabaseError(string message);

        // Single-player game
        string PromptPlayerName { get; }
        string FeedbackFetchingWord(string source);
        string ErrorCouldNotStartGame(string message);
        string FeedbackWonRound(int wins);
        string PromptContinuePlaying { get; }
        string FeedbackEndingLoop { get; }
        string FeedbackContinuing { get; }
        string FeedbackPressAnyKeyToSave { get; }
        string FeedbackHighscoreSaved(int wins, WordDifficulty difficulty);
        string FeedbackReturningToMenu { get; }
        string ErrorNoCustomWordsFound(WordDifficulty difficulty, WordLanguage language);

        // Tournament game
        string TournamentTitle { get; }
        string PromptPlayer1Name { get; }
        string PromptPlayer2Name { get; }
        string FeedbackTournamentStarting(string p1, string p2, string source, string firstGuesser, int lives);
        string FeedbackPressToStartRound { get; }
        string ErrorCouldNotFetchTournamentWord(string message);
        string FeedbackTournamentRoundEnded { get; }
        string FeedbackTournamentLives(string p1Name, int p1Lives, string p2Name, int p2Lives);
        string FeedbackTournamentNextGuesser(string guesserName);
        string FeedbackTournamentCancelled { get; }
        string FeedbackTournamentEnded { get; }
        string FeedbackTournamentDraw { get; }
        string FeedbackTournamentWinner(string winnerName);
        string FeedbackTournamentLoser(string loserName);
        string FeedbackTournamentFinalWins { get; }
        string FeedbackTournamentPlayerWins(string playerName, int wins);
        string FeedbackTournamentUnexpectedEnd { get; }

        // Highscores
        string HighscoreTitle { get; }
        string HighscoreFetching { get; }
        string HighscoreNoneFound { get; }
        string HighscoreDifficultyHeader(WordDifficulty difficulty);
        string HighscoreEntry(int rank, string name, int wins);

        // Add word
        string AddWordTitle { get; }
        string AddWordPrompt { get; }
        string AddWordInvalid { get; }
        string AddWordSuccess(string word, WordDifficulty difficulty, WordLanguage language);
        string AddWordErrorExists(string message);
        string AddWordSelectLanguageTitle { get; }
        string AddWordSelectLanguagePrompt { get; }
        string AddWordLanguageSwedish { get; }
        string AddWordLanguageEnglish { get; }
        string ErrorWordAlreadyExists(string word, WordDifficulty difficulty, WordLanguage language);

        // Help screen
        string HelpTitle { get; }
        string HelpLine1 { get; }
        string HelpLine2 { get; }
        string HelpModesTitle { get; }
        string HelpModesLine1 { get; }
        string HelpModesLine2 { get; }
        string HelpSourcesTitle { get; }
        string HelpSourcesLine1 { get; }
        string HelpSourcesLine2 { get; }
        string HelpSourcesLine3 { get; }

        // Game round
        string RoundTitleNewRound { get; }
        string RoundTitleGame { get; }
        string RoundActivePlayer { get; }
        string RoundActivePlayerWithLives(string playerName, int lives);
        string RoundWord { get; }
        string RoundMistakes(int mistakes, int max);
        string RoundGuessedLetters { get; }
        string RoundFeedbackCorrectGuess(char guess);
        string RoundFeedbackWrongGuess(char guess);
        string RoundFeedbackCancelled { get; }
        string RoundLost { get; }
        string RoundWon { get; }

        // End screen
        string EndScreenCongrats { get; }
        string EndScreenCancelled { get; }
        string EndScreenLost { get; }
        string EndScreenCorrectWord(string secret);

        // Input handling
        string GetGuessPrompt { get; }
        string GetGuessInvalid(char letter);
        string GetGuessAlreadyGuessed(char letter);
        string GetPlayerNameEmpty { get; }

        // Timer-related strings
        string RoundTimerDisplay(int seconds);
        string RoundTimerExpired { get; }

        // WPF-specific strings
        string ButtonBackToMenu { get; }
        string MainMenuWpfTitle { get; }
        string LanguageSelectorTitle { get; }
        string LanguageButtonSwedish { get; }
        string LanguageButtonEnglish { get; }
        string SettingsTitle { get; }
        string SettingsLabelDifficulty { get; }
        string SettingsLabelWordList { get; }
        string SettingsButtonStart { get; }
        string AddWordLabelLanguage { get; }
        string AddWordButtonSave { get; }
        string HighscoreHeaderName { get; }
        string HighscoreHeaderWins { get; }
        string HighscoreHeaderDifficulty { get; }
        string GameButtonGiveUp { get; }
        string GameButtonPlayAgain { get; }
        string GameButtonSaveAndExit { get; }
        string TournamentButtonNextRound { get; }
        string TournamentButtonCancel { get; }
        string ErrorApiGeneric { get; }
        string HighscoreStatusLoading { get; }
        string HighscoreStatusError(string message);
        string HighscoreStatusNoneFoundWpf { get; }
        string TournamentHeaderLives { get; }
        string TournamentHeaderWins { get; }

        // Default player names used by the WPF UI
        string DefaultPlayerName { get; }
        string DefaultPlayer1Name { get; }
        string DefaultPlayer2Name { get; }
    }
}
