using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class GenreService
    {
        private readonly Database _db;

        public GenreService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public List<string> ListGenre()
        {
            if (!_db.TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }
            return _db.Genres.Select(g => g.Name).ToList();
        }

        public (bool Success, string Message) GenreAdd(string name)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");
                if (string.IsNullOrWhiteSpace(name)) return (false, "Имя жанра не может быть пустым");

                if (_db.Genres.Any(g => g.Name == name)) return (false, $"Жанр '{name}' уже существует");
                var newGenre = new GenreFilms
                {
                    Idgenre = _db.Genres.Any() ? _db.Genres.Max(g => g.Idgenre) + 1 : 1,
                    Name = name
                };
                _db.Genres.Add(newGenre);
                _db.SaveChanges();
                return (true, $"Жанр '{name}' успешно добавлен");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении жанра: {ex.Message}");
            }
        }
    }
}