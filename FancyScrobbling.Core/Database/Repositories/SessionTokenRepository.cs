using FancyScrobbling.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core.Database.Repositories
{
    public class SessionTokenRepository
    {
        private readonly AppDbContext _context;

        public SessionTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SetSessionTokenAsync(Session session)
        {
            try
            {
                _context.SessionTokens.Update(session);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public Session GetSessionToken()
        {
            return _context.SessionTokens.FirstOrDefault();
        }

        public async Task<bool> RemoveSessionToken()
        {
            try
            {
                _context.SessionTokens.Remove(GetSessionToken());
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
