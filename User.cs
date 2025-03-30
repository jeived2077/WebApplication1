using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace WebApplication1
{
    public class AuthService
    {
        private readonly Database _db;
        private static readonly byte[] StaticSalt = Encoding.UTF8.GetBytes("MyStaticSalt1234567890");

        public AuthService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        //метод регистрации
        public (bool Success, string Token, string ErrorMessage) Registration(string login, string password)
        {
            try
            {
                if (!_db.TestConnection()) return (false, null, "Не удалось подключиться к базе данных");
                if (_db.User.Any(u => u.Login == login)) return (false, null, "Логин уже занят");
                if (password.Length < 8) return (false, null, "Пароль должен содержать минимум 8 символов");



                if (Regex.IsMatch(password, @"\p{IsCyrillic}")) return (false, null, "Пароль не должен содержать кириллицу");



                if (!Regex.IsMatch(password, @"[A-Z]")) return (false, null, "Пароль должен содержать хотя бы одну заглавную букву латинского алфавита");


                var newUser = new Users
                {
                    IdUser = _db.User.Any() ? _db.User.Max(u => u.IdUser) + 1 : 1,
                    Login = login,
                    Password = HashPassword(password),
                    Status = "user"
                };

                _db.User.Add(newUser);
                _db.SaveChanges();

                string token = GenerateJwtToken(newUser);
                return (true, token, null);
            }
            catch (Exception ex)
            {
                return (false, null, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        

        public static string HashPassword(string password)
        {
            string hashedPassword = Convert.ToBase64String(KeyDerivation.Pbkdf2(
                password: password!,
                salt: StaticSalt,
                prf: KeyDerivationPrf.HMACSHA256,
                iterationCount: 100000,
                numBytesRequested: 256 / 8));
            return hashedPassword;
        }
        //метод авторизации
        public (bool Success, string Token, string Login, string Status, string ErrorMessage) Authorize(string login, string password)
        {
            try
            {
                if (!_db.TestConnection()) return (false, null, null, null, "Не удалось подключиться к базе данных");
                var user = _db.User.FirstOrDefault(u => u.Login == login);
                if (user == null) return (false, null, null, null, "Пользователь не найден");

                string hashedInputPassword = HashPassword(password);
                if (hashedInputPassword != user.Password) return (false, null, null, null, "Неверный пароль");

                string token = GenerateJwtToken(user);
                return (true, token, user.Login, user.Status, null);
            }
            catch (Exception ex)
            {
                return (false, null, null, null, $"Ошибка при авторизации: {ex.Message}");
            }
        }
        //генераци токена
        private string GenerateJwtToken(Users user)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()),
                new Claim(ClaimTypes.Name, user.Login),
                new Claim(ClaimTypes.Role, user.Status)
            };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(1),
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }

        public void ListingUser()
        {
            var users = _db.User.ToList();
            Console.WriteLine("Users list:");
            foreach (Users u in users)
            {
                Console.WriteLine($"{u.Login}.{u.Password} - {u.Status}");
            }
        }
        public (bool IsValid, Dictionary<string, string> Claims, string ErrorMessage) DecodeJwtToken(string token)
        {
            try
            {
                var tokenHandler = new JwtSecurityTokenHandler();

                // Параметры валидации токена
                var validationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true, // Проверяем, не истек ли токен
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = AuthOptions.ISSUER,
                    ValidAudience = AuthOptions.AUDIENCE,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey()
                };

                // Валидация токена и получение claims
                SecurityToken validatedToken;
                var principal = tokenHandler.ValidateToken(token, validationParameters, out validatedToken);

                // Извлекаем claims в удобный формат (Dictionary)
                var claims = principal.Claims.ToDictionary(
                    claim => claim.Type,
                    claim => claim.Value
                );

                return (true, claims, null);
            }

            catch (SecurityTokenInvalidSignatureException ex)
            {
                return (false, null, "Неверная подпись токена: " + ex.Message);
            }
            catch (Exception ex)
            {
                return (false, null, "Ошибка при декодировании токена: " + ex.Message);
            }
        }
    }
}