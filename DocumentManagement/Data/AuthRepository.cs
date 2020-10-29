using DocumentManagement.Models.Database;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace DocumentManagement.Data
{
    public class AuthRepository : IAuthRepository
    {
        private readonly DocumentManagementContext context;      
        public AuthRepository(DocumentManagementContext context)
            => this.context = context;
       
        public async Task<UserDb> Register(UserDb user, string password)
        {
            CreatePasswordHash(password, out byte[] passwordHash, out byte[] passwordSalt);
            user.PasswordHash = passwordHash;
            user.PasswordSalt = passwordSalt;

            await context.Users.AddAsync(user);
            await context.SaveChangesAsync();

            return user;
        }

        public async Task<UserDb> Login(string login, string password)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                return null;

            if (!VerifyPasswordHash(password, user.PasswordHash, user.PasswordSalt))
                return null;

            return user;
        }
        public async Task<UserDb> Get(string login)
        {
            return await context.Users
                .Include(u => u.Projects)
                .Include(u => u.Tasks)
                .FirstOrDefaultAsync(u => u.Login == login);
        }

        /// <summary>
        /// TODO: Update user
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        public Task<UserDb> Update(UserDb project)
        {
            throw new System.NotImplementedException();
        }

        public async Task<bool> Delete(string login)
        {
            var user = await context.Users.FirstOrDefaultAsync(u => u.Login == login);

            if (user == null)
                return false;

            context.Users.Remove(user);
            await context.SaveChangesAsync();

            return !(await IsExists(login));
        }

        public async Task<bool> IsExists(string login)
        {
            if (await context.Users.AnyAsync(u => u.Login == login))
                return true;

            return false;
        }

        private bool VerifyPasswordHash(string password, byte[] passwordHash, byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512(passwordSalt))
            {
                var computedHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != passwordHash[i]) return false;
                }
            }

            return true;
        }
    
        private void CreatePasswordHash(string password, out byte[] passwordHash, out byte[] passwordSalt)
        {
            using (var hmac = new System.Security.Cryptography.HMACSHA512())
            {
                passwordSalt = hmac.Key;
                passwordHash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
            }
        }
    }
}