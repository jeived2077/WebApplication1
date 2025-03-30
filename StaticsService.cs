using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class StaticsService
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
        public StaticsService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        
        public List<object> ListGenresStatics()
        {
            var genres = from g in Genres
                         join fg in FilmToGenres on g.Idgenre equals fg.Idgenre
                         join fm in Movies on fg.Idfilms equals fm.Idfims
                         join vf in ViewFilms on fm.Idfims equals vf.Films
                         group fg by new { g.Name, g.Idgenre } into grouped
                         select new
                         {
                             CountsGenre = grouped.Count(),
                             Name = grouped.Key.Name,
                             IdGenre = grouped.Key.Idgenre
                         };

            return genres.ToList<object>();
        }
        
        
        public List<object> ListActorsStatics()
        {
            var actors = from a in Actors
                         join fta in FilmToActors on a.Idactor equals fta.Idactor
                         join fm in Movies on fta.Idfilms equals fm.Idfims
                         join vf in ViewFilms on fm.Idfims equals vf.Films
                         group fta by new { a.Name, a.Idactor } into grouped
                         select new
                         {
                             CountsActor = grouped.Count(),
                             Name = grouped.Key.Name,
                             IdActor = grouped.Key.Idactor
                         };

            return actors.ToList<object>();
        }

        public List<object> ListDirectorsStatics()
        {
            var directors = from d in Directors
                            join ftd in FilmToDirectors on d.Iddirector equals ftd.Iddirector
                            join fm in Movies on ftd.Idfilms equals fm.Idfims
                            join vf in ViewFilms on fm.Idfims equals vf.Films
                            group ftd by new { d.Name, d.Iddirector } into grouped
                            select new
                            {
                                CountsDirector = grouped.Count(),
                                Name = grouped.Key.Name,
                                IdDirector = grouped.Key.Iddirector
                            };

            return directors.ToList<object>();
        }

        public List<object> ListYearsStatics()
        {
            var years = from fm in Movies
                        join vf in ViewFilms on fm.Idfims equals vf.Films
                        group fm by fm.Years into grouped
                        select new
                        {
                            CountsYear = grouped.Count(),
                            Year = grouped.Key
                        };

            return years.ToList<object>();
        }
        
    }
}
