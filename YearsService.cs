using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class YearsService
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
        public YearsService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public List<string> YearsList()
        {
            if (!_db.TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }


            var years = Movies.Select(m => m.Years).ToList();



            return years;
        }
    }
}
