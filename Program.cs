using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebApplication1;

var builder = WebApplication.CreateBuilder();



builder.Services.AddDbContext<Database>(options =>
{
    string server = "localhost";
    int port = 3309; 
    string database = "filmsdb";
    string username = "root"; 
    string password = "root"; 
    string connectionString = $"Server={server};Port={port};Database={database};Uid={username};Pwd={password};";
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", builder =>
    {
        builder.AllowAnyOrigin()
               .AllowAnyMethod()
               .AllowAnyHeader();
    });
});

builder.WebHost.UseUrls("http://localhost:5184");


var app = builder.Build();

app.UseCors("AllowAll");
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseDefaultFiles(new DefaultFilesOptions
{
    DefaultFileNames = new List<string> { "homepage.html" } 
});

app.MapPost("/api/auth/register", async (HttpRequest request, AuthService auth) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];
        string password = data["password"];

        var (success, token, errorMessage) = auth.Registration(login, password);
        return success ? Results.Text(token) : Results.Text(errorMessage, statusCode: 400);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка обработки запроса: {ex.Message}", statusCode: 400);
    }
});


app.MapPost("/api/auth/login", async (HttpRequest request, AuthService auth) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];
        string password = data["password"];

        var (success, token, _, _, errorMessage) = auth.Authorize(login, password);
        return success ? Results.Text(token) : Results.Text(errorMessage, statusCode: 401);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка обработки запроса: {ex.Message}", statusCode: 400);
    }
});








app.MapPost("/api/genres", async (HttpRequest request, GenreService genre) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string name = data["name"];
        Console.WriteLine($"Добавление жанра: {name}");

        var (success, message) = genre.GenreAdd(name);
        if (success)
            return Results.Json(new { Success = true, Message = message });
        return Results.Json(new { Success = false, Message = message }, statusCode: 400);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при добавлении жанра: {ex.Message}");
        return Results.Json(new { Success = false, Message = $"Ошибка при добавлении жанра: {ex.Message}" }, statusCode: 500);
    }
});
app.MapPost("/api/actors", async (HttpRequest request, ActorsService actors) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string firstName = data["firstName"];
        string lastName = data["lastName"];
        Console.WriteLine($"Добавление актёра: {firstName} {lastName}");

        var (success, message) = actors.ActorAdd(firstName, lastName);
        if (success)
            return Results.Json(new { Success = true, Message = message });
        return Results.Json(new { Success = false, Message = message }, statusCode: 400);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при добавлении актёра: {ex.Message}");
        return Results.Json(new { Success = false, Message = $"Ошибка при добавлении актёра: {ex.Message}" }, statusCode: 500);
    }
});
app.MapPost("/api/directors", async (HttpRequest request, DirectorService director) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string firstName = data["firstName"];
        string lastName = data["lastName"];
        Console.WriteLine($"Добавление режиссёра: {firstName} {lastName}");

        var (success, message) = director.DirectorAdd(firstName, lastName);
        if (success)
            return Results.Json(new { Success = true, Message = message });
        return Results.Json(new { Success = false, Message = message }, statusCode: 400);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Ошибка при добавлении режиссёра: {ex.Message}");
        return Results.Json(new { Success = false, Message = $"Ошибка при добавлении режиссёра: {ex.Message}" }, statusCode: 500);
    }
});


app.MapGet("/api/movies", (FilmService film, ActorsService actorserv, GenreService genre, DirectorService director) =>
{
    try
    {
        var genres = genre.ListGenre();
        var directors = director.DirectorList();
        var actors = actorserv.ActorList(); 
        var movies = film.ListingFilms();

        var result = new
        {
            genres,
            directors,
            actors,
            movies
        };

        return Results.Ok(result);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении фильмов: {ex.Message}", statusCode: 500);
    }
});
app.MapPost("/api/movies", async (HttpRequest request,AuthService auth, FilmService film) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
        string title = data["title"].ToString();
        string description = data["description"]?.ToString();
        string poster = data["poster"]?.ToString();
        string year = data["year"].ToString();
        var genres = JsonSerializer.Deserialize<List<string>>(data["genres"].ToString()); // Исправлено с "genre" на "genres"
        var directors = JsonSerializer.Deserialize<List<string>>(data["directors"].ToString());
        var actors = JsonSerializer.Deserialize<List<string>>(data["actors"].ToString());
        int userId = int.Parse(claims[ClaimTypes.NameIdentifier]);

        film.FilmsAdd(title, description, poster, year, genres, directors, actors, userId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при добавлении фильма: {ex.Message}", statusCode: 500);
    }
});

app.MapGet("/api/movies/{id}", (int id, Database db) =>
{
    try
    {
        var movieDetails = db.GetMovieDetails(id);
        return Results.Ok(movieDetails);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении деталей фильма: {ex.Message}", statusCode: 500);
    }
});

app.MapPut("/api/movies/{id}", async (int id, HttpRequest request, FilmService film, AuthService auth) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, object>>(body);
        string title = data["title"].ToString();
        string description = data["description"]?.ToString();
        string poster = data["poster"]?.ToString();
        string year = data["year"].ToString();
        var genres = JsonSerializer.Deserialize<List<string>>(data["genres"].ToString()); // Исправлено с "genre" на "genres"
        var directors = JsonSerializer.Deserialize<List<string>>(data["directors"].ToString());
        var actors = JsonSerializer.Deserialize<List<string>>(data["actors"].ToString());

        film.EditingFilms(id, title, description, poster, year, genres, directors, actors);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при обновлении фильма: {ex.Message}", statusCode: 500);
    }
});


app.MapDelete("/api/movies/{id}", async (int id, HttpRequest request, AuthService auth, FilmService film) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        film.DeleteFilms(id);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при удалении фильма: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/actors", (ActorsService actor) =>
{
    try
    {
        var actors = actor.ActorList();
        return Results.Ok(actors);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении актеров: {ex.Message}", statusCode: 500);
    }
});
app.MapDelete("/api/movies/{movieId}/directors/{directorName}", async (int movieId, string directorName, HttpRequest request, AuthService auth, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var director = db.Directors.FirstOrDefault(d => d.Name == directorName);
        if (director == null) return Results.NotFound("Режиссер не найден");

        var filmToDirector = db.FilmToDirectors.FirstOrDefault(ftd => ftd.Idfilms == movieId && ftd.Iddirector == director.Iddirector);
        if (filmToDirector == null) return Results.NotFound("Привязка режиссера к фильму не найдена");

        db.FilmToDirectors.Remove(filmToDirector);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при удалении режиссера из фильма: {ex.Message}", statusCode: 500);
    }
});


app.MapDelete("/api/movies/{movieId}/actors/{actorName}", async (int movieId, string actorName, HttpRequest request, AuthService auth, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var actor = db.Actors.FirstOrDefault(a => a.Name == actorName);
        if (actor == null) return Results.NotFound("Актер не найден");

        var filmToActor = db.FilmToActors.FirstOrDefault(fta => fta.Idfilms == movieId && fta.Idactor == actor.Idactor);
        if (filmToActor == null) return Results.NotFound("Привязка актера к фильму не найдена");

        db.FilmToActors.Remove(filmToActor);
        await db.SaveChangesAsync();
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при удалении актера из фильма: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/statistics/genres", (StaticsService staticsService) =>
{
    try
    {
        var genreStats = staticsService.ListGenresStatics();
        return Results.Ok(genreStats);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении статистики по жанрам: {ex.Message}", statusCode: 500);
    }
});


app.MapGet("/api/statistics/directors", (StaticsService statics) =>
{
    try
    {
        var directorStats = statics.ListDirectorsStatics();
        return Results.Ok(directorStats);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении статистики по режиссерам: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/statistics/actors", (StaticsService statics) =>
{
    
    try
    {
        var actorStats = statics.ListActorsStatics();
        return Results.Ok(actorStats);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении статистики по актерам: {ex.Message}", statusCode: 500);
    }
});

app.MapGet("/api/movies/genres/{idgenres}", (string idgenres, StaticServiceFilms staticsServiceFilm) =>
{
    try
    {
        var movies = staticsServiceFilm.ListFilmsToGenres(idgenres);
        return Results.Ok(movies);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении списка фильмов по жанру: {ex.Message}", statusCode: 500);
    }
});


app.MapGet("/api/movies/actors/{idactors}", (string idactors, StaticsService statics, StaticServiceFilms staticsfilm) =>
{
    try
    {
        var movies = staticsfilm.ListActors(idactors);
        return Results.Ok(movies);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении списка фильмов по актеру: {ex.Message}", statusCode: 500);
    }
});


app.MapGet("/api/movies/years/{years}", (string years, StaticServiceFilms serviceFilms) =>
{
    try
    {
        var movies = serviceFilms.ListYears(years);
        return Results.Ok(movies);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении списка фильмов по году: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/movies/directors/{iddirector}", (string iddirector, StaticServiceFilms staticService) =>
{
    try
    {
        var movies = staticService.ListDirectors(iddirector);
        return Results.Ok(movies);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении списка фильмов по году: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/statistics/years", (StaticsService statics) =>
{
    try
    {
        var yearStats = statics.ListYearsStatics();
        return Results.Ok(yearStats);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении статистики по годам: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/movies/export", (XMLservice xmls) =>
{
    try
    {
        var xml = xmls.ExportFilmsToXML();
        return Results.Content(xml, "application/xml");
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при экспорте: {ex.Message}", statusCode: 500);
    }
});
app.MapPost("/api/movies/import", async (HttpRequest request, AuthService auth, XMLservice xmls) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

       
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var movieData = JsonSerializer.Deserialize<Dictionary<string, object>>(body);

        
        string title = movieData["title"].ToString();
        string poster = movieData.ContainsKey("poster") ? movieData["poster"].ToString() : null;
        string information = movieData.ContainsKey("information") ? movieData["information"].ToString() : null;
        int user = int.Parse(movieData["user"].ToString());
        string year = movieData.ContainsKey("year") ? movieData["year"].ToString() : null;

        var (success, message) = xmls.ImportFilmsToXML(title, poster, information, user, year);
        return Results.Json(new { Success = success, Message = message });
    }
    catch (Exception ex)
    {
        return Results.Json(new { Success = false, Message = $"Ошибка при импорте: {ex.Message}" }, statusCode: 500);
    }
});
app.MapGet("/api/users", async (HttpRequest request, AuthService auth, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var role = claims[ClaimTypes.Role];
        if (role != "admin") return Results.Text("Доступ запрещен: требуется роль администратора", statusCode: 403);

        var users = db.User.Select(u => new { u.IdUser, u.Login, u.Status }).ToList();
        return Results.Ok(users);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении пользователей: {ex.Message}", statusCode: 500);
    }
});

// Повышение статуса пользователя
app.MapPost("/api/users/promote", async (HttpRequest request, AuthService auth, AdminService admin) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var role = claims[ClaimTypes.Role];
        if (role != "admin") return Results.Text("Доступ запрещен: требуется роль администратора", statusCode: 403);

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];

        var (success, message) = admin.PromoteUser(login);
        return Results.Json(new { Success = success, Message = message }, statusCode: success ? 200 : 400);
    }
    catch (Exception ex)
    {
        return Results.Json(new { Success = false, Message = $"Ошибка при повышении статуса: {ex.Message}" }, statusCode: 500);
    }
});

// Понижение статуса пользователя
app.MapPost("/api/users/demote", async (HttpRequest request, AuthService auth, AdminService admin) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var role = claims[ClaimTypes.Role];
        if (role != "admin") return Results.Text("Доступ запрещен: требуется роль администратора", statusCode: 403);

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];

        var (success, message) = admin.DemoteUser(login);
        return Results.Json(new { Success = success, Message = message }, statusCode: success ? 200 : 400);
    }
    catch (Exception ex)
    {
        return Results.Json(new { Success = false, Message = $"Ошибка при понижении статуса: {ex.Message}" }, statusCode: 500);
    }
});

// Удаление пользователя
app.MapDelete("/api/users", async (HttpRequest request, AuthService auth, AdminService admin) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = auth.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        var role = claims[ClaimTypes.Role];
        if (role != "admin") return Results.Text("Доступ запрещен: требуется роль администратора", statusCode: 403);

        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];

        var (success, message) = admin.DeleteUser(login);
        return Results.Json(new { Success = success, Message = message }, statusCode: success ? 200 : 400);
    }
    catch (Exception ex)
    {
        return Results.Json(new { Success = false, Message = $"Ошибка при удалении пользователя: {ex.Message}" }, statusCode: 500);
    }
});
app.Run();