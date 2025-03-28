using Microsoft.IdentityModel.Tokens;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace WebApplication1
{
    public class User
    {
        [Key]
        public int IdUser { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public string Status { get; set; }
        public string? Jwttoken { get; set; }
    }

    public class Movie
    {
        [Key]
        public int Idfims { get; set; }
        public string? Namefilms { get; set; }
        public string Image { get; set; }
        public string Years { get; set; }
        public string Information { get; set; }
        public int Users { get; set; }
    }

    public class Actor
    {
        [Key]
        public int Idactor { get; set; }
        public string Name { get; set; }
    }

    public class GenreFilms
    {
        [Key]
        public int Idgenre { get; set; }
        public string Name { get; set; }
    }

    public class DirectorFilms
    {
        [Key]
        public int Iddirector { get; set; }
        public string Name { get; set; }
    }

    public class FilmToActor
    {
        public int Idactor { get; set; }
        public int? Idfilms { get; set; }
    }

    public class FilmToGenre
    {
        public int Idfilms { get; set; }
        public int Idgenre { get; set; }
        public virtual Movie Movie { get; set; }
        public virtual GenreFilms Genre { get; set; }
    }

    public class FilmToDirector
    {
        public int Iddirector { get; set; }
        public int? Idfilms { get; set; }
    }

    public class FavoriteFilms
    {
        [Key]
        public int Ididfilms { get; set; }
        public int? IdUser { get; set; }
    }

    public class ViewFilms
    {
        [Key]
        public int Id { get; set; }
        public int? Films { get; set; }
    }

    public class AuthOptions
    {
        public const string ISSUER = "MyAuthServer";
        public const string AUDIENCE = "MyAuthClient";
        const string KEY = "mysupersecret_secretsecretsecretkey!123";
        public static SymmetricSecurityKey GetSymmetricSecurityKey() =>
            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(KEY));
    }
}