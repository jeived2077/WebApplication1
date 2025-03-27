    using Microsoft.IdentityModel.Tokens;
    using System.Text;
    using System.ComponentModel.DataAnnotations;

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
            public int Idfims { get; set; } // Соответствует idfims
            public string Namefilms { get; set; }
            public byte[] Image { get; set; } // BLOB в базе требует byte[]
            public string Years { get; set; }
            public string Information { get; set; }
            public int? Genre { get; set; } // Внешний ключ на genre.idgenre
            public int? Users { get; set; } // Внешний ключ на Users.IdUser
        }

        public class Actor
        {
            [Key]
            public int Idactor { get; set; } // Соответствует idactor
            public string Name { get; set; }
        }

        public class GenreFilms
        {
            [Key]
            public int Idgenre { get; set; } // Соответствует idgenre
            public string Name { get; set; }
        }

        public class DirectorFilms
        {
            [Key]
            public int Iddirector { get; set; } // Соответствует iddirector
            public string Name { get; set; }
        }

        public class FilmToActor
        {
            [Key]
            public int Idactor { get; set; } // Первичный ключ из filmstoactor
            public int? Idfilms { get; set; } // Внешний ключ на fims.idfims
        }

        public class FilmToDirector
        {
            [Key]
            public int Iddirector { get; set; } // Первичный ключ из filmstodirector
            public int? Idfilms { get; set; } // Внешний ключ на fims.idfims
        }

        public class FavoriteFilms
        {
            [Key]
            public int Ididfilms { get; set; } // Первичный ключ из favoriteFilms
            public int? IdUser { get; set; } // Внешний ключ на Users.IdUser
        }

        public class ViewFilms
        {
            [Key]
            public int IdUser { get; set; } // Первичный ключ из ViewFilms
            public int? Films { get; set; } // Внешний ключ на fims.idfims
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