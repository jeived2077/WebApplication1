using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class StaticServiceFilms
    {
        
        private readonly Database _db;
        public StaticServiceFilms(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public List<object> ListFilmsToGenres(string idgenres)
        {
            var query = from g in _db.Genres
                        join fg in _db.FilmToGenres on g.Idgenre equals fg.Idgenre
                        join fm in _db.Movies on fg.Idfilms equals fm.Idfims
                        join vf in _db.ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(idgenres) || g.Idgenre.ToString() == idgenres)
                        select new
                        {
                            Film = fm,
                            Genres = _db.Genres
                                .Join(_db.FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = _db.Actors
                                .Join(_db.FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name)
                                .ToList(),
                            Directors = _db.Directors
                                .Join(_db.FilmToDirectors,
                                    d => d.Iddirector,
                                    ftd => ftd.Iddirector,
                                    (d, ftd) => new { d, ftd })
                                .Where(x => x.ftd.Idfilms == fm.Idfims)
                                .Select(x => x.d.Name)
                                .ToList()
                        };

            var result = query
                .GroupBy(x => x.Film.Idfims)
                .Select(g => new
                {
                    Idfims = g.Key,
                    Title = g.First().Film.Namefilms,
                    Year = g.First().Film.Years,
                    Poster = g.First().Film.Image,
                    Information = g.First().Film.Information,
                    Genres = g.First().Genres.Any() ? string.Join(", ", g.First().Genres) : "Нет жанров",
                    Actors = g.First().Actors.Any() ? string.Join(", ", g.First().Actors) : "Нет актёров",
                    Directors = g.First().Directors.Any() ? string.Join(", ", g.First().Directors) : "Нет режиссёров"
                });

            return result.ToList<object>();
        }
        public List<object> ListDirectors(string iddirectors)
        {
            var query = from d in _db.Directors
                        join ftd in _db.FilmToDirectors on d.Iddirector equals ftd.Iddirector
                        join fm in _db.Movies on ftd.Idfilms equals fm.Idfims
                        join vf in _db.ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(iddirectors) || d.Iddirector.ToString() == iddirectors)
                        select new
                        {
                            Film = fm,
                            Genres = _db.Genres
                                .Join(_db.FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name).ToList(),

                            Actors = _db.Actors
                                .Join(_db.FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name).ToList(),
                            Directors = _db.Directors
                                .Join(_db.FilmToDirectors,
                                    dir => dir.Iddirector,
                                    ftd => ftd.Iddirector,
                                    (dir, ftd) => new { dir, ftd })
                                .Where(x => x.ftd.Idfilms == fm.Idfims)
                                .Select(x => x.dir.Name).ToList(),
                        };

            var result = query
                .GroupBy(x => x.Film.Idfims)
                .Select(g => new
                {
                    Idfims = g.Key,
                    Title = g.First().Film.Namefilms,
                    Year = g.First().Film.Years,
                    Poster = g.First().Film.Image,
                    Information = g.First().Film.Information,
                    Genres = g.First().Genres.Any() ? string.Join(", ", g.First().Genres) : "Нет жанров",
                    Actors = g.First().Actors.Any() ? string.Join(", ", g.First().Actors) : "Нет актёров",
                    Directors = g.First().Directors.Any() ? string.Join(", ", g.First().Directors) : "Нет режиссёров"
                });

            return result.ToList<object>();
        }
        public List<object> ListYears(string years)
        {
            var query = from fm in _db.Movies
                        join vf in _db.ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(years) || fm.Years == years)
                        select new
                        {

                            Film = fm,
                            Genres = _db.Genres
                                .Join(_db.FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = _db.Actors
                                .Join(_db.FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name)
                                .ToList(),
                            Directors = _db.Directors
                                .Join(_db.FilmToDirectors,
                                    d => d.Iddirector,
                                    ftd => ftd.Iddirector,
                                    (d, ftd) => new { d, ftd })
                                .Where(x => x.ftd.Idfilms == fm.Idfims)
                                .Select(x => x.d.Name)
                                .ToList()
                        };

            var result = query
                .GroupBy(x => x.Film.Idfims)
                .Select(g => new
                {
                    Idfims = g.Key,
                    Title = g.First().Film.Namefilms,
                    Year = g.First().Film.Years,
                    Poster = g.First().Film.Image,
                    Information = g.First().Film.Information,
                    Genres = g.First().Genres.Any() ? string.Join(", ", g.First().Genres) : "Нет жанров",
                    Actors = g.First().Actors.Any() ? string.Join(", ", g.First().Actors) : "Нет актёров",
                    Directors = g.First().Directors.Any() ? string.Join(", ", g.First().Directors) : "Нет режиссёров"
                });

            return result.ToList<object>();
        }
        public List<object> ListActors(string idactors)
        {
            var query = from a in _db.Actors
                        join fta in _db.FilmToActors on a.Idactor equals fta.Idactor
                        join fm in _db.Movies on fta.Idfilms equals fm.Idfims
                        join vf in _db.ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(idactors) || a.Idactor.ToString() == idactors)
                        select new
                        {
                            Film = fm,
                            Genres = _db.Genres
                                .Join(_db.FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = _db.Actors
                                .Join(_db.FilmToActors,
                                    act => act.Idactor,
                                    fta => fta.Idactor,
                                    (act, fta) => new { act, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.act.Name)
                                .ToList(),
                            Directors = _db.Directors
                                .Join(_db.FilmToDirectors,
                                    d => d.Iddirector,
                                    ftd => ftd.Iddirector,
                                    (d, ftd) => new { d, ftd })
                                .Where(x => x.ftd.Idfilms == fm.Idfims)
                                .Select(x => x.d.Name)
                                .ToList(),
                        };

            var result = query
                .GroupBy(x => x.Film.Idfims)
                .Select(g => new
                {
                    Idfims = g.Key,
                    Title = g.First().Film.Namefilms,
                    Year = g.First().Film.Years,
                    Poster = g.First().Film.Image,
                    Information = g.First().Film.Information,
                    Genres = g.First().Genres.Any() ? string.Join(", ", g.First().Genres) : "Нет жанров",
                    Actors = g.First().Actors.Any() ? string.Join(", ", g.First().Actors) : "Нет актёров",
                    Directors = g.First().Directors.Any() ? string.Join(", ", g.First().Directors) : "Нет режиссёров"
                });

            return result.ToList<object>();
        }
    }
}
