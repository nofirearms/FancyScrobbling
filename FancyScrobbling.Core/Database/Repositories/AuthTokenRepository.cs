using FancyScrobbling.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FancyScrobbling.Core.Database.Repositories
{
    public class AuthTokenRepository
    {
        private readonly AppDbContext _context;

        public AuthTokenRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> SetAuthTokenAsync(AuthToken token)
        {
            try
            {
                _context.AuthTokens.Update(token);
                await _context.SaveChangesAsync();
                return true;
            }
            catch
            {
                return false;
            }
        }

        public AuthToken GetAuthToken()
        {
            return _context.AuthTokens.LastOrDefault();
        }

        public async Task<bool> RemoveSessionToken()
        {
            try
            {
                _context.AuthTokens.Remove(GetAuthToken());
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
