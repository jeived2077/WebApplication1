using Microsoft.AspNetCore.Cryptography.KeyDerivation;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.IO;
using System.Security.Claims;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace WebApplication1
{
    public class Database : DbContext
    {
        public DbSet<User> User { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<GenreFilms> Genres { get; set; }
        public DbSet<DirectorFilms> Directors { get; set; }
        public DbSet<FilmToActor> FilmToActors { get; set; }
        public DbSet<FilmToDirector> FilmToDirectors { get; set; }
        public DbSet<FavoriteFilms> FavoriteFilms { get; set; }
        public DbSet<ViewFilms> ViewFilms { get; set; }

        public Database(DbContextOptions<Database> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("Users");
            modelBuilder.Entity<Movie>().ToTable("fims");
            modelBuilder.Entity<Actor>().ToTable("actor");
            modelBuilder.Entity<GenreFilms>().ToTable("genre");
            modelBuilder.Entity<DirectorFilms>().ToTable("director");
            modelBuilder.Entity<FilmToActor>().ToTable("filmstoactor").HasKey(f => new { f.Idactor, f.Idfilms });
            modelBuilder.Entity<FilmToDirector>().ToTable("filmstodirector").HasKey(f => new { f.Iddirector, f.Idfilms });
            modelBuilder.Entity<FavoriteFilms>().ToTable("favoriteFilms");
            modelBuilder.Entity<ViewFilms>().ToTable("ViewFilms").HasKey(v => new { v.IdUser, v.Films });
        }

        public bool TestConnection()
        {
            try
            {
                Database.CanConnect();
                Console.WriteLine("Подключение к бд успешно");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Подключение к бд с ошибкой: {ex.Message}");
                return false;
            }
        }
        //метод регистрации
        public (bool Success, string Token, string ErrorMessage) Registration(string login, string password)
        {
            try
            {
                if (!TestConnection()) return (false,null, "Не удалось подключиться к базе данных");
                if (User.Any(u => u.Login == login)) return (false,null, "Логин уже занят");
                if (password.Length < 8) return (false, null, "Пароль должен содержать минимум 8 символов");



                if (Regex.IsMatch(password, @"\p{IsCyrillic}")) return (false, null, "Пароль не должен содержать кириллицу");



                if (!Regex.IsMatch(password, @"[A-Z]")) return (false, null, "Пароль должен содержать хотя бы одну заглавную букву латинского алфавита");


                var newUser = new User
                {
                    IdUser = User.Any() ? User.Max(u => u.IdUser) + 1 : 1,
                    Login = login,
                    Password = HashPassword(password),
                    Status = "user"
                };

                User.Add(newUser);
                SaveChanges();

                string token = GenerateJwtToken(newUser);
                return (true, token, null);
            }
            catch (Exception ex)
            {
                return (false,null, $"Ошибка при регистрации: {ex.Message}");
            }
        }

        private static readonly byte[] StaticSalt = Encoding.UTF8.GetBytes("MyStaticSalt1234567890"); // Статическая соль
                                                                                                                                                                                                 
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
                if (!TestConnection()) return (false, null, null, null, "Не удалось подключиться к базе данных");
                var user = User.FirstOrDefault(u => u.Login == login);
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
        private string GenerateJwtToken(User user)
        {
            var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.IdUser.ToString()), // ID пользователя
        new Claim(ClaimTypes.Name, user.Login),                      // Логин
        new Claim(ClaimTypes.Role, user.Status)                      // Статус
    };

            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.AddMonths(1), // Увеличиваем срок действия до 1 часа
                signingCredentials: new SigningCredentials(
                    AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return new JwtSecurityTokenHandler().WriteToken(jwt);
        }
        // метод чтения пользователей и статуса
        public void ListingUser()
        {
            var users = User.ToList();
            Console.WriteLine("Users list:");
            foreach (User u in users)
            {
                Console.WriteLine($"{u.Login}.{u.Password} - {u.Status}");
            }
        }
        // метод чтения фильмов как список
        public List<object> ListingFilms()
        {
            try
            {
                if (!TestConnection())
                {
                    Console.WriteLine("Не удалось подключиться к базе данных");
                    return new List<object>();
                }

                var films = from m in Movies
                            join g in Genres on m.Genre equals g.Idgenre into genreGroup
                            from g in genreGroup.DefaultIfEmpty()
                            join u in User on m.Users equals u.IdUser into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Film = m,
                                Genre = g != null ? g.Name : "Нет жанра",
                                Actors = (from fta in FilmToActors
                                          join a in Actors on fta.Idactor equals a.Idactor
                                          where fta.Idfilms == m.Idfims
                                          select a.Name).Distinct().ToList(),
                                Directors = (from ftd in FilmToDirectors
                                             join d in Directors on ftd.Iddirector equals d.Iddirector
                                             where ftd.Idfilms == m.Idfims
                                             select d.Name).Distinct().ToList(),
                                ViewCount = ViewFilms.Count(vf => vf.Films == m.Idfims),
                                CreatorLogin = u != null ? u.Login : "Неизвестно"
                            };

                var result = films.ToList().Select(f => new
                {
                    id = f.Film.Idfims, // Используем "id" в нижнем регистре для соответствия клиенту
                    title = f.Film.Namefilms,
                    year = f.Film.Years,
                    description = f.Film.Information,
                    poster = f.Film.Image != null ? $"data:image/jpeg;base64,{Convert.ToBase64String(f.Film.Image)}" : null,
                    genre = f.Genre,
                    actors = f.Actors.Any() ? string.Join(", ", f.Actors) : "Нет актёров",
                    directors = f.Directors.Any() ? string.Join(", ", f.Directors) : "Нет режиссёров",
                    viewCount = f.ViewCount,
                    creator = f.CreatorLogin
                }).ToList<object>();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка фильмов: {ex.Message}");
                return new List<object>();
            }
        }
        public List<string> ListGenre()
        {
            if (!TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }
            var genre = Genres.Select(g => g.Name).ToList();
            
            return genre;
        }
        public List<string> DirectorList()
        {
            if (!TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }

            var director = Directors.Select(g => g.Name).ToList();
            

            return director;
        }
        public List<string> YearsList()
        {
            if (!TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>(); 
            }

            
            var years = Movies.Select(m => m.Years).ToList();

            

            return years;
        }
        public void DirectorAdd(string firstName, string lastName)
        {
            var newDirector = new DirectorFilms
            {
                Iddirector = Directors.Any() ? Directors.Max(d => d.Iddirector) + 1 : 1, 
                Name = $"{firstName} {lastName}"
            };
            Directors.Add(newDirector);
            SaveChanges(); 
        }

        public void ActorAdd(string firstName, string lastName)
        {
            var newActor = new Actor
            {
                Idactor = Actors.Any() ? Actors.Max(a => a.Idactor) + 1 : 1, 
                Name = $"{firstName} {lastName}"
            };
            Actors.Add(newActor);
            SaveChanges(); 
        }

        public void GenreAdd(string name)
        {
            var newGenre = new GenreFilms
            {
                Idgenre = Genres.Any() ? Genres.Max(g => g.Idgenre) + 1 : 1, // Уникальный ID
                Name = name
            };
            Genres.Add(newGenre);
            SaveChanges(); // Сохранение изменений
        }

        public List<string> ActorList()
        {
            if (!TestConnection()) return new List<string>();
            return Actors.Select(a => a.Name).ToList();
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
        public void FilmsAdd(string title, string description, string poster, string year, string genre, List<string> directors, List<string> actors, int userId)
        {
            try
            {
                if (!TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var genreEntity = Genres.FirstOrDefault(g => g.Name == genre);
                if (genreEntity == null)
                {
                    genreEntity = new GenreFilms { Idgenre = Genres.Any() ? Genres.Max(g => g.Idgenre) + 1 : 1, Name = genre };
                    Genres.Add(genreEntity);
                    SaveChanges();
                }

                var movie = new Movie
                {
                    Idfims = Movies.Any() ? Movies.Max(m => m.Idfims) + 1 : 1,
                    Namefilms = title,
                    Information = description,
                    Image = poster != null ? Convert.FromBase64String(poster.Split(',')[1]) : null, // Предполагается base64
                    Years = year,
                    Genre = genreEntity.Idgenre,
                    Users = userId
                };
                Movies.Add(movie);
                SaveChanges();

                foreach (var directorName in directors)
                {
                    var director = Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director != null)
                    {
                        FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = movie.Idfims });
                    }
                }

                foreach (var actorName in actors)
                {
                    var actor = Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor != null)
                    {
                        FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = movie.Idfims });
                    }
                }
                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении фильма: {ex.Message}");
                throw;
            }
        }
        public void EditingFilms(int id, string title, string description, string poster, string year, string genre, List<string> directors, List<string> actors)
        {
            try
            {
                if (!TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");

                var genreEntity = Genres.FirstOrDefault(g => g.Name == genre);
                if (genreEntity == null)
                {
                    genreEntity = new GenreFilms { Idgenre = Genres.Any() ? Genres.Max(g => g.Idgenre) + 1 : 1, Name = genre };
                    Genres.Add(genreEntity);
                    SaveChanges();
                }

                movie.Namefilms = title;
                movie.Information = description;
                movie.Image = poster != null ? Convert.FromBase64String(poster.Split(',')[1]) : null;
                movie.Years = year;
                movie.Genre = genreEntity.Idgenre;

                // Обновляем режиссеров
                var existingDirectors = FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
                foreach (var ftd in existingDirectors)
                {
                    if (!directors.Contains(Directors.First(d => d.Iddirector == ftd.Iddirector).Name))
                    {
                        FilmToDirectors.Remove(ftd);
                    }
                }
                foreach (var directorName in directors)
                {
                    var director = Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director != null && !FilmToDirectors.Any(ftd => ftd.Idfilms == id && ftd.Iddirector == director.Iddirector))
                    {
                        FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = id });
                    }
                }

                // Обновляем актеров
                var existingActors = FilmToActors.Where(fta => fta.Idfilms == id).ToList();
                foreach (var fta in existingActors)
                {
                    if (!actors.Contains(Actors.First(a => a.Idactor == fta.Idactor).Name))
                    {
                        FilmToActors.Remove(fta);
                    }
                }
                foreach (var actorName in actors)
                {
                    var actor = Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor != null && !FilmToActors.Any(fta => fta.Idfilms == id && fta.Idactor == actor.Idactor))
                    {
                        FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = id });
                    }
                }

                SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при редактировании фильма: {ex.Message}");
                throw;
            }
        }
        public void DeleteFilms(int id)
{
    try
    {
        if (!TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

        var movie = Movies.FirstOrDefault(m => m.Idfims == id);
        if (movie == null) throw new Exception("Фильм не найден");

        var filmToDirectors = FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
        var filmToActors = FilmToActors.Where(fta => fta.Idfilms == id).ToList();
        var viewFilms = ViewFilms.Where(vf => vf.Films == id).ToList();

        FilmToDirectors.RemoveRange(filmToDirectors);
        FilmToActors.RemoveRange(filmToActors);
        ViewFilms.RemoveRange(viewFilms);
        Movies.Remove(movie);
        SaveChanges();
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при удалении фильма: {ex.Message}");
        throw;
    }
}
        public void FilmsDetails() { }
    }
} 