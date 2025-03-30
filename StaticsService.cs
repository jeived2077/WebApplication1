using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class StaticsService
    {
       
        private readonly Database _db;
        public StaticsService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        
        public List<object> ListGenresStatics()
        {
            var genres = from g in _db.Genres
                         join fg in _db.FilmToGenres on g.Idgenre equals fg.Idgenre
                         join fm in _db.Movies on fg.Idfilms equals fm.Idfims
                         join vf in _db.ViewFilms on fm.Idfims equals vf.Films
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
            var actors = from a in _db.Actors
                         join fta in _db.FilmToActors on a.Idactor equals fta.Idactor
                         join fm in _db.Movies on fta.Idfilms equals fm.Idfims
                         join vf in _db.ViewFilms on fm.Idfims equals vf.Films
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
            var directors = from d in _db.Directors
                            join ftd in _db.FilmToDirectors on d.Iddirector equals ftd.Iddirector
                            join fm in _db.Movies on ftd.Idfilms equals fm.Idfims
                            join vf in _db.ViewFilms on fm.Idfims equals vf.Films
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
            var years = from fm in _db.Movies
                        join vf in _db.ViewFilms on fm.Idfims equals vf.Films
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
