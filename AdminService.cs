using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class AdminService
    {
        public DbSet<Users> User { get; set; }
        public DbSet<Movie> Movies { get; set; }
        public DbSet<Actor> Actors { get; set; }
        public DbSet<GenreFilms> Genres { get; set; }
        public DbSet<DirectorFilms> Directors { get; set; }
        public DbSet<FilmToActor> FilmToActors { get; set; }
        public DbSet<FilmToDirector> FilmToDirectors { get; set; }
        public DbSet<FavoriteFilms> FavoriteFilms { get; set; }
        public DbSet<ViewFilms> ViewFilms { get; set; }
        public DbSet<FilmToGenre> FilmToGenres { get; set; }
        private readonly Database _db;
        FilmService film;
        public AdminService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public (bool Success, string Message) PromoteUser(string login)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");

                var user = User.FirstOrDefault(u => u.Login == login);
                if (user == null) return (false, "Пользователь не найден");

                if (user.Status == "admin") return (false, "Пользователь уже администратор");

                user.Status = "admin";
                _db.SaveChanges();
                return (true, $"Пользователь {login} повышен до администратора");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при повышении статуса: {ex.Message}");
            }
        }
        public (bool Success, string Message) DemoteUser(string login)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");

                var user = User.FirstOrDefault(u => u.Login == login);
                if (user == null) return (false, "Пользователь не найден");

                if (user.Status == "user") return (false, "Пользователь уже имеет минимальный статус");

                user.Status = "user";
                _db.SaveChanges();
                return (true, $"Пользователь {login} понижен до обычного пользователя");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при понижении статуса: {ex.Message}");
            }
        }

        public (bool Success, string Message) DeleteUser(string login)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");

                var user = User.FirstOrDefault(u => u.Login == login);
                if (user == null) return (false, "Пользователь не найден");

                
                var userMovies = Movies.Where(m => m.Users == user.IdUser).ToList();
                foreach (var movie in userMovies)
                {
                    film.DeleteFilms(movie.Idfims);
                }

                User.Remove(user);
                _db.SaveChanges();
                return (true, $"Пользователь {login} успешно удален");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при удалении пользователя: {ex.Message}");
            }
        }
    }
}
