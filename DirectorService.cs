using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class DirectorService
    {
        private readonly Database _db;

        public DirectorService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public List<string> DirectorList()
        {
            if (!_db.TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }
            return _db.Directors.Select(d => d.Name).ToList();
        }

        public (bool Success, string Message) DirectorAdd(string firstName, string lastName)
        {
            try
            {
                string fullName = $"{firstName} {lastName}".Trim();
                if (string.IsNullOrEmpty(fullName)) return (false, "Данные режиссёра не может быть пустым");
                if (_db.Directors.Any(d => d.Name == fullName)) return (false, $"Режиссёр '{fullName}' уже существует"); // Check Directors, not Actors
                var newDirector = new DirectorFilms
                {
                    Iddirector = _db.Directors.Any() ? _db.Directors.Max(d => d.Iddirector) + 1 : 1,
                    Name = fullName
                };
                _db.Directors.Add(newDirector);
                _db.SaveChanges();
                return (true, $"Режиссёр '{fullName}' успешно добавлен"); // Fixed message
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении режиссёра: {ex.Message}");
            }
        }
    }
} 