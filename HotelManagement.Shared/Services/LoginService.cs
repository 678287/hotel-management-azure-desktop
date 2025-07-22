using HotelManagement.Shared.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace HotelManagement.Shared.Services
{
    public class LoginService
    {
        private readonly HotelDbContext _context;
        private readonly PasswordHasher<IdentityUser> _hasher = new();

        public IdentityUser? LoggedInUser { get; private set; } 

        public LoginService(HotelDbContext context)
        {
            _context = context;
        }

        public async Task<IdentityUser?> LoginAsync(string email, string password)
        {
            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == email);
            if (user == null) return null;

            var result = _hasher.VerifyHashedPassword(user, user.PasswordHash, password);

            if (result == PasswordVerificationResult.Success)
            {
                LoggedInUser = user;
                return user;
            }

            return null;
        }
    }
}
