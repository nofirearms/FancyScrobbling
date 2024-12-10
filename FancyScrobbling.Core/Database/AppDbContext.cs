using FancyScrobbling.Core.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core.Database
{
    public class AppDbContext : DbContext
    {
        public DbSet<AuthToken> AuthTokens { get; set; }
        public DbSet<Session> SessionTokens { get; set; }

        public AppDbContext()
        {
            Database.EnsureCreated();
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseSqlite("Data source=data.db");
        }
    }
}
