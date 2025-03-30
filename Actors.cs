using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class ActorsService
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
                if (Actors.Any(a => a.Name == fullName)) return (false, $"Актёр '{fullName}' уже существует");

                var newActor = new Actor
                {

                    Name = fullName
                };
                Actors.Add(newActor);
                _db.SaveChanges();
                return (true, $"Актёр '{fullName}' успешно добавлен");
            }
            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении режиссёра: {ex.Message}");
            }
        }
        public List<string> ActorList()
        {
            if (!_db.TestConnection()) return new List<string>();
            return Actors.Select(a => a.Name).ToList();
        }
        
    }
}
