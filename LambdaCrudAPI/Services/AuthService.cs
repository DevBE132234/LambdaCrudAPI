using LambdaCrudAPI.Interfaces;
using LambdaCrudAPI.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Collections;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace LambdaCrudAPI.Services
{
    public class AuthService : IAuthService
    {

        private readonly AdventureWorksDbContext _context;
        private readonly IConfiguration _config;

        public AuthService(AdventureWorksDbContext context)
        {
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        public string Authenticate(string email, string password)
        {
            var user = _context.People
                .Join(_context.EmailAddresses,
                      person => person.BusinessEntityId,
                      emailTable => emailTable.BusinessEntityId,
                      (person, emailTable) => new { person, emailTable })
                .Join(_context.Passwords,
                      combined => combined.person.BusinessEntityId,
                      passwordTable => passwordTable.BusinessEntityId,
                      (combined, passwordTable) => new { combined.person, combined.emailTable, passwordTable })
                .FirstOrDefault(u => u.emailTable.EmailAddress1 == email);

            if (user == null)
                return null; // Email not found

            // Convert PasswordSalt from Base64 string to byte[]
            byte[] storedSalt = Convert.FromBase64String(user.passwordTable.PasswordSalt);
            byte[] storedHash = Convert.FromBase64String(user.passwordTable.PasswordHash);

            if (!VerifyPassword(password, Convert.ToBase64String(storedHash), Convert.ToBase64String(storedSalt)))
                return null; // Authentication failed

            // Generate JWT Token with correct email
            return GenerateJwtToken(user.person, user.emailTable.EmailAddress1);
        }
        public static string ComputeSha256Hash(string password, string salt)
        {
            using (SHA256 sha256Hash = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password + salt);
                byte[] hashBytes = sha256Hash.ComputeHash(bytes);
                return Convert.ToBase64String(hashBytes);
            }
        }

        private bool VerifyPassword(string enteredPassword, string storedHash, string storedSalt)
        {
            // Convert storedSalt (Base64) back to byte array
            byte[] saltBytes = Convert.FromBase64String(storedSalt);

            // Compute hash using the same salt
            using (var hmac = new HMACSHA256(saltBytes))
            {
                byte[] computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(enteredPassword));
                string computedHashBase64 = Convert.ToBase64String(computedHash);

                // Compare computed hash with stored hash
                return computedHashBase64 == storedHash;
            }
        }

        private string GenerateJwtToken(Person user, string email)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("YourSuperSecretKeyThatIsAtLeast16Characters"));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
        new Claim(ClaimTypes.NameIdentifier, user.BusinessEntityId.ToString()), // Fixed property name
        new Claim(ClaimTypes.Email, email) // Directly pass the email from Authenticate()
    };

            var token = new JwtSecurityToken(
                //issuer: _config["Jwt:Issuer"],
                //audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public bool Register(string email, string password, string firstName, string lastName, string personType)
        {
            // Check if email already exists
            var existingUser = _context.EmailAddresses.FirstOrDefault(e => e.EmailAddress1 == email);
            if (existingUser != null)
                return false; // Email already registered

            // ✅ Validate PersonType (optional)
            var allowedPersonTypes = new List<string> { "IN", "SP", "EM", "SC", "VC" }; // Adjust as per database
            if (!allowedPersonTypes.Contains(personType))
                throw new ArgumentException("Invalid PersonType");

            // Create BusinessEntity entry (required for Person)
            var businessEntity = new BusinessEntity
            {
                ModifiedDate = DateTime.UtcNow
            };
            _context.BusinessEntities.Add(businessEntity);
            _context.SaveChanges(); // Save to get BusinessEntityId

            // Hash password
            CreatePasswordHash(password, out string passwordHash, out string passwordSalt);

            // Create new Person record
            var person = new Person
            {
                BusinessEntityId = businessEntity.BusinessEntityId, // Use generated ID
                FirstName = firstName,
                LastName = lastName,
                PersonType = personType, // 👈 Set value from request
                ModifiedDate = DateTime.UtcNow
            };
            _context.People.Add(person);
            _context.SaveChanges();

            // Create EmailAddress entry
            var emailAddress = new EmailAddress
            {
                BusinessEntityId = person.BusinessEntityId,
                EmailAddress1 = email
            };
            _context.EmailAddresses.Add(emailAddress);

            // Create Password entry
            var passwordEntry = new Password
            {
                BusinessEntityId = person.BusinessEntityId,
                PasswordHash = passwordHash,
                PasswordSalt =passwordSalt, // ✅ No need for Substring(0,8)
                ModifiedDate = DateTime.UtcNow
            };
            _context.Passwords.Add(passwordEntry);

            // Save all changes
            _context.SaveChanges();

            return true;
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            // Generate exactly 6 bytes of random salt
            byte[] saltBytes = new byte[6];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(saltBytes);
            }

            // Convert salt to Base64 (exactly 8 chars)
            passwordSalt = Convert.ToBase64String(saltBytes);

            // Hash the password using the salt as key
            using (var hmac = new HMACSHA256(saltBytes))
            {
                byte[] passwordHashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));
                passwordHash = Convert.ToBase64String(passwordHashBytes);
            }
        }

    }
}
