using Dapper;
using Microsoft.AspNetCore.Identity;
using MySql;
using MySql.Data.MySqlClient;
using TrainingChatWebApp;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello-world", () => "Hello World!");
app.MapPost("/login", (LoginDto login) =>
{
    var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
    var user = connection.Query<User>(
        @"
        SELECT * FROM TrainingChatApp.Users
        WHERE Username = @Username
        LIMIT 1
        ",
        new { username = login.username }).FirstOrDefault();
    if (user is null)
    {
        return Results.Unauthorized(); //401
    }
    var session = new Session
    {
        UserId = user.Id,
        SessionId = Guid.NewGuid()
    };

    connection.Execute(@"
        INSERT INTO TrainingChatApp.Sessions
        VALUES(@SessionId, @UserId, NULL)
    ",
    new { session.SessionId, UserId = session.UserId });
    return Results.Ok(new {SessionId = session.SessionId});



});



app.Run();