using Microsoft.EntityFrameworkCore;
using System.Text;
using System.Xml.Linq;

namespace WebApplication1
{
    public class XMLservice
    {
        
        private readonly Database _db;
        public XMLservice(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public (bool Success, string Error) ImportFilmsToXML(string title, string poster, string information, int user, string year)
        {
            try
            {
                if (!_db.TestConnection())
                    throw new Exception("Не удалось подключиться к базе данных");

                if (string.IsNullOrWhiteSpace(title))
                    throw new ArgumentException("Название фильма не может быть пустым");
                if (string.IsNullOrWhiteSpace(year))
                    throw new ArgumentException("Год не может быть пустым");

                Console.WriteLine($"Размер данных для Information: {Encoding.UTF8.GetByteCount(information)} байт");

                bool filmExists = _db.Movies.Any(m => m.Namefilms == title && m.Years == year);
                if (filmExists)
                    return (false, $"Фильм '{title}' ({year}) уже существует на сайте");

                var movie = new Movie
                {
                    Namefilms = title,
                    Information = information ?? "",
                    Image = poster ?? "",
                    Years = year,
                    Users = user
                };
                _db.Movies.Add(movie);
                _db.SaveChanges();

                return (true, $"Фильм '{title}' успешно импортирован");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при добавлении фильма: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Внутреннее исключение: {ex.InnerException.Message}");
                }
                return (false, $"Ошибка при импорте: {ex.Message}");
            }
        }
        public string ExportFilmsToXML()
        {
            try
            {
                if (!_db.TestConnection())
                {
                    Console.WriteLine("Не удалось подключиться к базе данных");
                    return "<error>Не удалось подключиться к базе данных</error>";
                }

                var movies = _db.Movies.Select(m => new
                {
                    m.Idfims,
                    m.Namefilms,
                    m.Years,
                    m.Information,
                    m.Image,
                    Creator = m.Users
                }).ToList();

                var xml = new XDocument(
                    new XElement("movies",
                        movies.Select(m => new XElement("movie",
                            new XElement("title", m.Namefilms),
                            new XElement("year", m.Years),
                            new XElement("information", m.Information),
                            new XElement("poster", m.Image),
                            new XElement("creator", m.Creator)
                        ))
                    )
                );

                return xml.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при экспорте фильмов: {ex.Message}");
                return $"<error>Ошибка при экспорте: {ex.Message}</error>";
            }
        }
    }
}
