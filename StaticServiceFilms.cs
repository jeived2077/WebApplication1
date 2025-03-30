using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class StaticServiceFilms
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
        public StaticServiceFilms(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public List<object> ListFilmsToGenres(string idgenres)
        {
            var query = from g in Genres
                        join fg in FilmToGenres on g.Idgenre equals fg.Idgenre
                        join fm in Movies on fg.Idfilms equals fm.Idfims
                        join vf in ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(idgenres) || g.Idgenre.ToString() == idgenres)
                        select new
                        {
                            Film = fm,
                            Genres = Genres
                                .Join(FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = Actors
                                .Join(FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name)
                                .ToList(),
                            Directors = Directors
                                .Join(FilmToDirectors,
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
            var query = from d in Directors
                        join ftd in FilmToDirectors on d.Iddirector equals ftd.Iddirector
                        join fm in Movies on ftd.Idfilms equals fm.Idfims
                        join vf in ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(iddirectors) || d.Iddirector.ToString() == iddirectors)
                        select new
                        {
                            Film = fm,
                            Genres = Genres
                                .Join(FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name).ToList(),

                            Actors = Actors
                                .Join(FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name).ToList(),
                            Directors = Directors
                                .Join(FilmToDirectors,
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
            var query = from fm in Movies
                        join vf in ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(years) || fm.Years == years)
                        select new
                        {

                            Film = fm,
                            Genres = Genres
                                .Join(FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = Actors
                                .Join(FilmToActors,
                                    a => a.Idactor,
                                    fta => fta.Idactor,
                                    (a, fta) => new { a, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.a.Name)
                                .ToList(),
                            Directors = Directors
                                .Join(FilmToDirectors,
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
            var query = from a in Actors
                        join fta in FilmToActors on a.Idactor equals fta.Idactor
                        join fm in Movies on fta.Idfilms equals fm.Idfims
                        join vf in ViewFilms on fm.Idfims equals vf.Films
                        where (string.IsNullOrEmpty(idactors) || a.Idactor.ToString() == idactors)
                        select new
                        {
                            Film = fm,
                            Genres = Genres
                                .Join(FilmToGenres,
                                    gen => gen.Idgenre,
                                    ftg => ftg.Idgenre,
                                    (gen, ftg) => new { gen, ftg })
                                .Where(x => x.ftg.Idfilms == fm.Idfims)
                                .Select(x => x.gen.Name)
                                .ToList(),
                            Actors = Actors
                                .Join(FilmToActors,
                                    act => act.Idactor,
                                    fta => fta.Idactor,
                                    (act, fta) => new { act, fta })
                                .Where(x => x.fta.Idfilms == fm.Idfims)
                                .Select(x => x.act.Name)
                                .ToList(),
                            Directors = Directors
                                .Join(FilmToDirectors,
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
