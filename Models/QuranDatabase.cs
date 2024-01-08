using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuranIndexMaker.Models
{
    public class QuranDatabase : DbContext
    {
        //public DbSet<Quran> Suralar { get; set; }
        public DbSet<Quran> quran { get; set; }
        public DbSet<SurahAyahLink> SurahAyahLinks { get; set; }
        public DbSet<SearchResult> SearchResults { get; set; }
        public DbSet<UniqueRootWord> UniqueRootWords { get; set; }

        public QuranDatabase()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder) =>
            optionsBuilder.UseSqlite(
           @"Data Source=quran_dynamic.db");
    }
}
