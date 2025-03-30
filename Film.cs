using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Xml.Linq;

namespace WebApplication1
{
    public class FilmService
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

        public FilmService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public List<object> ListingFilms()
        {
            try
            {
                if (!_db.TestConnection())
                {
                    Console.WriteLine("Не удалось подключиться к базе данных");
                    return new List<object>();
                }

                var films = from m in Movies
                            join u in User on m.Users equals u.IdUser into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Film = m,
                                Genres = (from ftg in FilmToGenres
                                          join g in Genres on ftg.Idgenre equals g.Idgenre
                                          where ftg.Idfilms == m.Idfims
                                          select g.Name).ToList(),
                                Actors = (from fta in FilmToActors
                                          join a in Actors on fta.Idactor equals a.Idactor
                                          where fta.Idfilms == m.Idfims
                                          select a.Name).Distinct().ToList(),
                                Directors = (from ftd in FilmToDirectors
                                             join d in Directors on ftd.Iddirector equals d.Iddirector
                                             where ftd.Idfilms == m.Idfims
                                             select d.Name).Distinct().ToList(),
                                ViewCount = ViewFilms.Count(vf => vf.Films == m.Idfims),
                                CreatorLogin = u != null ? u.Login : "Неизвестно"
                            };

                var result = films.ToList().Select(f => new
                {
                    id = f.Film.Idfims,
                    title = f.Film.Namefilms,
                    year = f.Film.Years,
                    description = f.Film.Information,
                    poster = f.Film.Image,
                    genres = f.Genres.Any() ? string.Join(", ", f.Genres) : "Нет жанров",
                    actors = f.Actors.Any() ? string.Join(", ", f.Actors) : "Нет актёров",
                    directors = f.Directors.Any() ? string.Join(", ", f.Directors) : "Нет режиссёров",
                    viewCount = f.ViewCount,
                    creator = f.CreatorLogin
                }).ToList<object>();

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении списка фильмов: {ex.Message}");
                return new List<object>();
            }
        }
        public void DeleteFilms(int id)
        {
            try
            {
                if (!_db.TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");

                var filmToDirectors = FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
                var filmToActors = FilmToActors.Where(fta => fta.Idfilms == id).ToList();
                var filmToGenres = FilmToGenres.Where(ftg => ftg.Idfilms == id).ToList();
                var viewFilms = ViewFilms.Where(vf => vf.Films == id).ToList();

                FilmToDirectors.RemoveRange(filmToDirectors);
                FilmToActors.RemoveRange(filmToActors);
                FilmToGenres.RemoveRange(filmToGenres);
                ViewFilms.RemoveRange(viewFilms);
                Movies.Remove(movie);
                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при удалении фильма: {ex.Message}");
                throw;
            }
        }
        public void FilmsAdd(string title, string description, string poster, string year, List<string> genres, List<string> directors, List<string> actors, int userId)
        {
            try
            {
                if (!_db.TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = new Movie
                {
                    Idfims = Movies.Any() ? Movies.Max(m => m.Idfims) + 1 : 1,
                    Namefilms = title,
                    Information = description,
                    Image = poster,
                    Years = year,
                    Users = userId
                };
                Movies.Add(movie);
                _db.SaveChanges();

                foreach (var genreName in genres)
                {
                    var genre = Genres.FirstOrDefault(g => g.Name == genreName);
                    if (genre == null)
                    {
                        genre = new GenreFilms
                        {
                            Idgenre = Genres.Any() ? Genres.Max(g => g.Idgenre) + 1 : 1,
                            Name = genreName
                        };
                        Genres.Add(genre);
                        _db.SaveChanges();
                    }

                    if (!FilmToGenres.Any(ftg => ftg.Idfilms == movie.Idfims && ftg.Idgenre == genre.Idgenre))
                    {
                        FilmToGenres.Add(new FilmToGenre { Idfilms = movie.Idfims, Idgenre = genre.Idgenre });
                    }
                }

                foreach (var directorName in directors)
                {
                    var director = Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director == null)
                    {
                        director = new DirectorFilms
                        {

                            Name = directorName
                        };
                        Directors.Add(director);
                        _db.SaveChanges();
                    }

                    if (!FilmToDirectors.Any(ftd => ftd.Idfilms == movie.Idfims && ftd.Iddirector == director.Iddirector))
                    {
                        FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = movie.Idfims });
                    }
                }

                foreach (var actorName in actors)
                {
                    var actor = Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor == null)
                    {
                        actor = new Actor
                        {
                            Idactor = Actors.Any() ? Actors.Max(a => a.Idactor) + 1 : 1,
                            Name = actorName
                        };
                        Actors.Add(actor);
                        _db.SaveChanges();
                    }

                    if (!FilmToActors.Any(fta => fta.Idfilms == movie.Idfims && fta.Idactor == actor.Idactor))
                    {
                        FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = movie.Idfims });
                    }
                }

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении фильма: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                }
                throw;
            }
        }
        public void EditingFilms(int id, string title, string description, string poster, string year, List<string> genres, List<string> directors, List<string> actors)
        {
            try
            {
                if (!_db.TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");

                movie.Namefilms = title;
                movie.Information = description;
                movie.Image = poster;
                movie.Years = year;


                var existingGenres = FilmToGenres.Where(ftg => ftg.Idfilms == id).ToList();
                FilmToGenres.RemoveRange(existingGenres);
                foreach (var genreName in genres)
                {
                    var genre = Genres.FirstOrDefault(g => g.Name == genreName);
                    if (genre == null)
                    {
                        genre = new GenreFilms
                        {
                            Idgenre = Genres.Any() ? Genres.Max(g => g.Idgenre) + 1 : 1,
                            Name = genreName
                        };
                        Genres.Add(genre);
                        _db.SaveChanges();
                    }
                    FilmToGenres.Add(new FilmToGenre { Idfilms = id, Idgenre = genre.Idgenre });
                }


                var existingDirectors = FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
                FilmToDirectors.RemoveRange(existingDirectors);
                foreach (var directorName in directors)
                {
                    var director = Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director != null)
                    {
                        FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = id });
                    }
                }


                var existingActors = FilmToActors.Where(fta => fta.Idfilms == id).ToList();
                FilmToActors.RemoveRange(existingActors);
                foreach (var actorName in actors)
                {
                    var actor = Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor != null)
                    {
                        FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = id });
                    }
                }

                _db.SaveChanges();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при редактировании фильма: {ex.Message}");
                throw;
            }
        }

    }
}