using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Hangman.Core.Models;
using System.IO;

namespace Hangman.Core.Providers.Db
{
    /// <summary>
    /// Provides the Entity Framework Core database context for high scores and custom words.
    /// </summary>
    public class HangmanDbContext : DbContext
    {
        public DbSet<HighscoreEntry> Highscores { get; set; }
        public DbSet<CustomWordEntry> CustomWords { get; set; }

        private readonly string _databasePath;

        public HangmanDbContext()
        {
            string baseDir = AppContext.BaseDirectory;
            _databasePath = Path.Combine(baseDir, "Hangman.db");

            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite($"Data Source={_databasePath}");
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // A player should have only one high-score record per difficulty level,
            // preventing multiple records from representing the same logical ranking entry.
            modelBuilder.Entity<HighscoreEntry>()
                .HasIndex(h => new { h.PlayerName, h.Difficulty })
                .IsUnique();

            // The same word may legitimately exist in different languages or difficulty levels,
            // but an identical combination must only be stored once to prevent duplicate custom words.
            modelBuilder.Entity<CustomWordEntry>()
                .HasIndex(w => new { w.Word, w.Difficulty, w.Language })
                .IsUnique();
        }
    }
}
