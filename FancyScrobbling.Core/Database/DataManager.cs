using FancyScrobbling.Core.Database.Repositories;

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
        /// <summary>
        /// Seems like it doesn't work at all
        /// </summary>
        public void ClearDatabase()
        {
            _context.ChangeTracker.Clear();
            _context.SaveChanges();
        }

    }
}
