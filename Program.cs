using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using WebApplication1;

var builder = WebApplication.CreateBuilder();



builder.Services.AddDbContext<Database>(options =>
{
    string server = "localhost";
    int port = 3307; 
    string database = "filmsdb";
    string username = "root"; 
    string password = ""; 
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

//маршрут регистрации
app.MapPost("/api/auth/register", async (HttpRequest request, Database db) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];
        string password = data["password"];

        var (success, token, errorMessage) = db.Registration(login, password);
        return success ? Results.Text(token) : Results.Text(errorMessage, statusCode: 400);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка обработки запроса: {ex.Message}", statusCode: 400);
    }
});

//маршрут авторизации
app.MapPost("/api/auth/login", async (HttpRequest request, Database db) =>
{
    try
    {
        using var reader = new StreamReader(request.Body);
        var body = await reader.ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string login = data["login"];
        string password = data["password"];

        var (success, token, _, _, errorMessage) = db.Authorize(login, password);
        return success ? Results.Text(token) : Results.Text(errorMessage, statusCode: 401);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка обработки запроса: {ex.Message}", statusCode: 400);
    }
});

//маршрут вывода пользователей
app.MapGet("/api/users", (Database db) =>
{
    var users = db.User.Select(u => new { u.Login, u.Status }).ToList();
    return Results.Ok(users);
});



app.MapGet("/api/users", (Database db) =>
{
    var users = db.User.Select(u => new { u.Login, u.Status }).ToList();
    return Results.Ok(users);
});

app.MapPost("/api/genres", async (HttpRequest request, Database db) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string name = data["name"];
        Console.WriteLine($"Добавление жанра: {name}");

        var (success, message) = db.GenreAdd(name);
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
app.MapPost("/api/actors", async (HttpRequest request, Database db) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string firstName = data["firstName"];
        string lastName = data["lastName"];
        Console.WriteLine($"Добавление актёра: {firstName} {lastName}");

        var (success, message) = db.ActorAdd(firstName, lastName);
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
app.MapPost("/api/directors", async (HttpRequest request, Database db) =>
{
    try
    {
        var body = await new StreamReader(request.Body).ReadToEndAsync();
        var data = JsonSerializer.Deserialize<Dictionary<string, string>>(body);
        string firstName = data["firstName"];
        string lastName = data["lastName"];
        Console.WriteLine($"Добавление режиссёра: {firstName} {lastName}");

        var (success, message) = db.DirectorAdd(firstName, lastName);
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

//маршрут вывода фильмов
app.MapGet("/api/movies", (Database db) =>
{
    try
    {
        var genres = db.ListGenre();
        var directors = db.DirectorList();
        var actors = db.ActorList(); // Добавляем актеров
        var movies = db.ListingFilms();

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
app.MapPost("/api/movies", async (HttpRequest request, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, claims, errorMessage) = db.DecodeJwtToken(token);
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

        db.FilmsAdd(title, description, poster, year, genres, directors, actors, userId);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при добавлении фильма: {ex.Message}", statusCode: 500);
    }
});
// Получение деталей фильма по ID
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

app.MapPut("/api/movies/{id}", async (int id, HttpRequest request, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = db.DecodeJwtToken(token);
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

        db.EditingFilms(id, title, description, poster, year, genres, directors, actors);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при обновлении фильма: {ex.Message}", statusCode: 500);
    }
});

// Удаление фильма
app.MapDelete("/api/movies/{id}", async (int id, HttpRequest request, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = db.DecodeJwtToken(token);
        if (!isValid) return Results.Text($"Ошибка авторизации: {errorMessage}", statusCode: 401);

        db.DeleteFilms(id);
        return Results.Ok();
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при удалении фильма: {ex.Message}", statusCode: 500);
    }
});
app.MapGet("/api/actors", (Database db) =>
{
    try
    {
        var actors = db.ActorList();
        return Results.Ok(actors);
    }
    catch (Exception ex)
    {
        return Results.Text($"Ошибка при получении актеров: {ex.Message}", statusCode: 500);
    }
});
app.MapDelete("/api/movies/{movieId}/directors/{directorName}", async (int movieId, string directorName, HttpRequest request, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = db.DecodeJwtToken(token);
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

// Удаление актера из фильма
app.MapDelete("/api/movies/{movieId}/actors/{actorName}", async (int movieId, string actorName, HttpRequest request, Database db) =>
{
    try
    {
        var token = request.Headers["Authorization"].ToString().Replace("Bearer ", "");
        var (isValid, _, errorMessage) = db.DecodeJwtToken(token);
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





app.Run();