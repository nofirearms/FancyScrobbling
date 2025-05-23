﻿using FancyScrobbling.Core.Models;

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
            return _context.SessionTokens.OrderBy(o => o.DateCreated).LastOrDefault();
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
