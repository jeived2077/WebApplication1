using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class DirectorService
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

            var director = Directors.Select(g => g.Name).ToList();


            return director;
        }
        public (bool Success, string Message) DirectorAdd(string firstName, string lastName)
        {
            try
            {
                string fullName = $"{firstName} {lastName}".Trim();
                if (string.IsNullOrEmpty(fullName)) return (false, "Данные режиссёра не может быть пустым");
                if (Actors.Any(a => a.Name == fullName)) return (false, $"Режиссёр '{fullName}' уже существует");
                var newDirector = new DirectorFilms
                {

                    Name = $"{firstName} {lastName}"
                };
                Directors.Add(newDirector);
                _db.SaveChanges();
                return (true, $"Актёр '{fullName}' успешно добавлен");
            }


            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении режиссёра: {ex.Message}");
            }
        }
    }
}
