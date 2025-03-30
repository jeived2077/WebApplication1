using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class AdminService
    {
        private readonly Database _db;
        private readonly FilmService _filmService;

        public AdminService(Database db, FilmService filmService)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
            _filmService = filmService ?? throw new ArgumentNullException(nameof(filmService));
        }

        public (bool Success, string Message) PromoteUser(string login)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");

                var user = _db.User.FirstOrDefault(u => u.Login == login);
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

                var user = _db.User.FirstOrDefault(u => u.Login == login);
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

                var user = _db.User.FirstOrDefault(u => u.Login == login);
                if (user == null) return (false, "Пользователь не найден");

                var userMovies = _db.Movies.Where(m => m.Users == user.IdUser).ToList();
                foreach (var movie in userMovies)
                {
                    _filmService.DeleteFilms(movie.Idfims);
                }

                _db.User.Remove(user);
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