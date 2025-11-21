using System.Linq;
using TPV_Gastronomico.Data;
using TPV_Gastronomico.Models;
using TPV_Gastronomico.Utils;

namespace TPV_Gastronomico.Services
{
    public class AuthService
    {
        private readonly DatabaseContext _db;
        public AuthService(DatabaseContext db) { _db = db; }

        public User Authenticate(string username, string password)
        {
            var user = _db.Users.SingleOrDefault(u => u.Username == username);
            if (user == null) return null;
            return PasswordHasher.Verify(user.PasswordHash, password) ? user : null;
        }
    }
}
