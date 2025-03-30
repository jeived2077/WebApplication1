using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class ActorsService
    {
        private readonly Database _db;

        public ActorsService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }

        public (bool Success, string Message) ActorAdd(string firstName, string lastName)
        {
            try
            {
                string fullName = $"{firstName} {lastName}".Trim();
                if (string.IsNullOrEmpty(fullName)) return (false, "Данные актёра не может быть пустым");
                if (_db.Actors.Any(a => a.Name == fullName)) return (false, $"Актёр '{fullName}' уже существует");

                var newActor = new Actor
                {
                    Idactor = _db.Actors.Any() ? _db.Actors.Max(a => a.Idactor) + 1 : 1,
                    Name = fullName
                };
                _db.Actors.Add(newActor);
                _db.SaveChanges();
                return (true, $"Актёр '{fullName}' успешно добавлен");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении актёра: {ex.Message}"); // Fixed message
            }
        }

        public List<string> ActorList()
        {
            if (!_db.TestConnection()) return new List<string>();
            return _db.Actors.Select(a => a.Name).ToList();
        }
    }
}