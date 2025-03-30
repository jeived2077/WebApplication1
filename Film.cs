using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class FilmService
    {
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

                var films = from m in _db.Movies
                            join u in _db.User on m.Users equals u.IdUser into userGroup
                            from u in userGroup.DefaultIfEmpty()
                            select new
                            {
                                Film = m,
                                Genres = (from ftg in _db.FilmToGenres
                                          join g in _db.Genres on ftg.Idgenre equals g.Idgenre
                                          where ftg.Idfilms == m.Idfims
                                          select g.Name).ToList(),
                                Actors = (from fta in _db.FilmToActors
                                          join a in _db.Actors on fta.Idactor equals a.Idactor
                                          where fta.Idfilms == m.Idfims
                                          select a.Name).Distinct().ToList(),
                                Directors = (from ftd in _db.FilmToDirectors
                                             join d in _db.Directors on ftd.Iddirector equals d.Iddirector
                                             where ftd.Idfilms == m.Idfims
                                             select d.Name).Distinct().ToList(),
                                ViewCount = _db.ViewFilms.Count(vf => vf.Films == m.Idfims),
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
                throw; // Propagate the exception to the endpoint
            }
        }

        public void DeleteFilms(int id)
        {
            try
            {
                if (!_db.TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = _db.Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");

                var filmToDirectors = _db.FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
                var filmToActors = _db.FilmToActors.Where(fta => fta.Idfilms == id).ToList();
                var filmToGenres = _db.FilmToGenres.Where(ftg => ftg.Idfilms == id).ToList();
                var viewFilms = _db.ViewFilms.Where(vf => vf.Films == id).ToList();

                _db.FilmToDirectors.RemoveRange(filmToDirectors);
                _db.FilmToActors.RemoveRange(filmToActors);
                _db.FilmToGenres.RemoveRange(filmToGenres);
                _db.ViewFilms.RemoveRange(viewFilms);
                _db.Movies.Remove(movie);
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
                    Idfims = _db.Movies.Any() ? _db.Movies.Max(m => m.Idfims) + 1 : 1,
                    Namefilms = title,
                    Information = description,
                    Image = poster,
                    Years = year,
                    Users = userId
                };
                _db.Movies.Add(movie);
                _db.SaveChanges();

                foreach (var genreName in genres)
                {
                    var genre = _db.Genres.FirstOrDefault(g => g.Name == genreName);
                    if (genre == null)
                    {
                        genre = new GenreFilms
                        {
                            Idgenre = _db.Genres.Any() ? _db.Genres.Max(g => g.Idgenre) + 1 : 1,
                            Name = genreName
                        };
                        _db.Genres.Add(genre);
                        _db.SaveChanges();
                    }

                    if (!_db.FilmToGenres.Any(ftg => ftg.Idfilms == movie.Idfims && ftg.Idgenre == genre.Idgenre))
                    {
                        _db.FilmToGenres.Add(new FilmToGenre { Idfilms = movie.Idfims, Idgenre = genre.Idgenre });
                    }
                }

                foreach (var directorName in directors)
                {
                    var director = _db.Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director == null)
                    {
                        director = new DirectorFilms
                        {
                            Iddirector = _db.Directors.Any() ? _db.Directors.Max(d => d.Iddirector) + 1 : 1,
                            Name = directorName
                        };
                        _db.Directors.Add(director);
                        _db.SaveChanges();
                    }

                    if (!_db.FilmToDirectors.Any(ftd => ftd.Idfilms == movie.Idfims && ftd.Iddirector == director.Iddirector))
                    {
                        _db.FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = movie.Idfims });
                    }
                }

                foreach (var actorName in actors)
                {
                    var actor = _db.Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor == null)
                    {
                        actor = new Actor
                        {
                            Idactor = _db.Actors.Any() ? _db.Actors.Max(a => a.Idactor) + 1 : 1,
                            Name = actorName
                        };
                        _db.Actors.Add(actor);
                        _db.SaveChanges();
                    }

                    if (!_db.FilmToActors.Any(fta => fta.Idfilms == movie.Idfims && fta.Idactor == actor.Idactor))
                    {
                        _db.FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = movie.Idfims });
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

                var movie = _db.Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");

                movie.Namefilms = title;
                movie.Information = description;
                movie.Image = poster;
                movie.Years = year;

                var existingGenres = _db.FilmToGenres.Where(ftg => ftg.Idfilms == id).ToList();
                _db.FilmToGenres.RemoveRange(existingGenres);
                foreach (var genreName in genres)
                {
                    var genre = _db.Genres.FirstOrDefault(g => g.Name == genreName);
                    if (genre == null)
                    {
                        genre = new GenreFilms
                        {
                            Idgenre = _db.Genres.Any() ? _db.Genres.Max(g => g.Idgenre) + 1 : 1,
                            Name = genreName
                        };
                        _db.Genres.Add(genre);
                        _db.SaveChanges();
                    }
                    _db.FilmToGenres.Add(new FilmToGenre { Idfilms = id, Idgenre = genre.Idgenre });
                }

                var existingDirectors = _db.FilmToDirectors.Where(ftd => ftd.Idfilms == id).ToList();
                _db.FilmToDirectors.RemoveRange(existingDirectors);
                foreach (var directorName in directors)
                {
                    var director = _db.Directors.FirstOrDefault(d => d.Name == directorName);
                    if (director != null)
                    {
                        _db.FilmToDirectors.Add(new FilmToDirector { Iddirector = director.Iddirector, Idfilms = id });
                    }
                }

                var existingActors = _db.FilmToActors.Where(fta => fta.Idfilms == id).ToList();
                _db.FilmToActors.RemoveRange(existingActors);
                foreach (var actorName in actors)
                {
                    var actor = _db.Actors.FirstOrDefault(a => a.Name == actorName);
                    if (actor != null)
                    {
                        _db.FilmToActors.Add(new FilmToActor { Idactor = actor.Idactor, Idfilms = id });
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