using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class GenreService
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
            var genre = Genres.Select(g => g.Name).ToList();

            return genre;
        }
        public (bool Success, string Message) GenreAdd(string name)
        {
            try
            {
                if (!_db.TestConnection()) return (false, "Не удалось подключиться к базе данных");
                if (string.IsNullOrWhiteSpace(name)) return (false, "Имя жанра не может быть пустым");


                if (Genres.Any(g => g.Name == name)) return (false, $"Жанр '{name}' уже существует");
                var newGenre = new GenreFilms
                {

                    Name = name
                };
                Genres.Add(newGenre);
                _db.SaveChanges();
                return (true, $"Жанр '{name}' успешно добавлен");
            }



            catch (Exception ex)
            {
                return (false, $"Ошибка при добавлении режиссёра: {ex.Message}");
            }
        }
    }
}
