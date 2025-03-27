

using Microsoft.AspNetCore.Builder;
using System;

var builder = WebApplication.CreateBuilder();
var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();



app.MapGet("/api/auth/login", (string id) =>
{
   
});
app.MapGet("/api/statistics/genres"), (string id) =>
{

});
app.MapGet("/api/statistics/directors"), (string id) =>
{

});
app.MapPost("/api/movies/imports"), (string id) =>
{

});
app.MapGet("/api/movies/export"), (string id) =>
{

});
app.MapGet("/api/users", (string id) =>
{

});
app.MapPut("/api/movies/{id}", (string id) =>
{

});
app.MapDelete("/api/movies/{id}"), (string id) =>
{

});

app.MapGet("GET /api/movies", (string id) =>
{

});




app.MapPost("/api/auth/register", () => {

    
});




app.Run();

