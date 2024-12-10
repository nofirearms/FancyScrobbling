using FancyScrobbling.Core.Database.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core.Database
{
    public class DataManager
    {
        private readonly AppDbContext _context;

        public AuthTokenRepository AuthTokenRepository { get; set; }
        public SessionTokenRepository SessionTokenRepository { get; set; }

        public DataManager()
        {
            _context = new AppDbContext();
            AuthTokenRepository = new AuthTokenRepository(_context);
            SessionTokenRepository = new SessionTokenRepository(_context);
        }
    }
}
