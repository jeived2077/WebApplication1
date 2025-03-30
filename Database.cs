        using Microsoft.AspNetCore.Cryptography.KeyDerivation;
        using Microsoft.EntityFrameworkCore;
        using Microsoft.IdentityModel.Tokens;
        using System.IdentityModel.Tokens.Jwt;
        using System.IO;
        using System.Security.Claims;
        using System.Security.Cryptography.X509Certificates;
        using System.Text;
        using System.Text.RegularExpressions;
        using System.Xml.Linq;

namespace WebApplication1
{
    public class Database : DbContext
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

        public Database(DbContextOptions<Database> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Users>().ToTable("Users");
            modelBuilder.Entity<Movie>().ToTable("fims")
                .Property(m => m.Image).HasColumnType("MEDIUMTEXT");
            modelBuilder.Entity<Movie>().ToTable("fims")
                .Property(m => m.Information).HasColumnType("MEDIUMTEXT"); 
            modelBuilder.Entity<Actor>().ToTable("actor");
            modelBuilder.Entity<GenreFilms>().ToTable("genre");
            modelBuilder.Entity<DirectorFilms>().ToTable("director");
            modelBuilder.Entity<FilmToActor>()
                .ToTable("filmstoactor")
                .HasKey(f => new { f.Idactor, f.Idfilms });
            modelBuilder.Entity<FilmToDirector>()
                .ToTable("filmstodirector")
                .HasKey(f => new { f.Iddirector, f.Idfilms });
            modelBuilder.Entity<FavoriteFilms>().ToTable("favoriteFilms");
            modelBuilder.Entity<ViewFilms>().ToTable("ViewFilms").HasKey(v => v.Id);
            modelBuilder.Entity<FilmToGenre>()
                .ToTable("filmstogenre")
                .HasKey(ftg => new { ftg.Idfilms, ftg.Idgenre });

            modelBuilder.Entity<FilmToGenre>()
                .HasOne(ftg => ftg.Movie)
                .WithMany()
                .HasForeignKey(ftg => ftg.Idfilms);

            modelBuilder.Entity<FilmToGenre>()
                .HasOne(ftg => ftg.Genre)
                .WithMany()
                .HasForeignKey(ftg => ftg.Idgenre);
        }

        public bool TestConnection()
        {
            try
            {
                Database.CanConnect();
                Console.WriteLine("Подключение к бд успешно");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Подключение к бд с ошибкой: {ex.Message}");
                return false;
            }
        }
        
        
        
        
        
        
       
        

        


        


        

        
        
        public object GetMovieDetails(int id)
        {
            try
            {
                if (!TestConnection()) throw new Exception("Не удалось подключиться к базе данных");

                var movie = Movies.FirstOrDefault(m => m.Idfims == id);
                if (movie == null) throw new Exception("Фильм не найден");


                var newView = new ViewFilms
                {
                    
                    Films = id
                };
                ViewFilms.Add(newView);
                SaveChanges();

                var genres = (from ftg in FilmToGenres
                              join g in Genres on ftg.Idgenre equals g.Idgenre
                              where ftg.Idfilms == id
                              select g.Name).ToList();

                var directors = (from ftd in FilmToDirectors
                                 join d in Directors on ftd.Iddirector equals d.Iddirector
                                 where ftd.Idfilms == id
                                 select d.Name).Distinct().ToList();

                var actors = (from fta in FilmToActors
                              join a in Actors on fta.Idactor equals a.Idactor
                              where fta.Idfilms == id
                              select a.Name).Distinct().ToList();

                var viewCount = ViewFilms.Count(vf => vf.Films == id);

                var creator = User.FirstOrDefault(u => u.IdUser == movie.Users)?.Login ?? "Неизвестно";

                return new
                {
                    id = movie.Idfims,
                    title = movie.Namefilms,
                    description = movie.Information,
                    poster = movie.Image,
                    year = movie.Years,
                    genres = genres.Any() ? string.Join(", ", genres) : "Нет жанров",
                    directors = directors.Any() ? string.Join(", ", directors) : "Нет режиссёров",
                    actors = actors.Any() ? string.Join(", ", actors) : "Нет актёров",
                    viewCount = viewCount,
                    creator = creator
                };
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Ошибка при получении деталей фильма: {ex.Message}");
                throw;
            }
        }

        

        
        
        
    }





}
    
