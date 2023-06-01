using System.Data;
using System.ComponentModel.Design;
using TrainingChatWebApp;
using MySql.Data.MySqlClient;
using Dapper;

// Criar uma aplicação Criar um app ASP.NET Core com minimal API.

// [X] Deve retornar uma página com em um endpoint '/hello-world' que diz "Hello World";
// [] Criar um endpoint POST que recebe um JSON para login de usuario, mas somente com o nome do usuário;
// [X] Criar uma tabela de usuários no DB;
// [X] Criar uma tabela de sessão no DB;
// [X] Criar um usuário no DB;
// [] Toda vez que receber um POST de login, válidar o usuário e criar uma entrada nova de sessão nova no DB;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddTransient(x =>
  new MySqlConnection(builder.Configuration.GetConnectionString("Default")));


var app = builder.Build();  

//Method Get
app.MapGet("/hello-world", () => "Hello World!");

//Method Post Login
app.MapPost("/login", (LoginDto login) => {
    using var scope = app.Services.CreateScope();
    var connection = scope.ServiceProvider.GetService<MySqlConnection>();

    var user = connection.Query<LoginUser> (
      @"
        SELECT *
        FROM TrainingChatApp.Users
        WHERE Username = @username
        LIMIT 1
      ", new {username = login.Username}).FirstOrDefault();

      if(user == null)
      {
        return Results.Unauthorized();
      }

    
    var session = new LoginSession
    {
      UserKey = user.Key,
      SessionId = Guid.NewGuid(),
    };

    connection.Execute(@"
    INSERT INTO TrainingChatApp.Session
    VALUES (NULL, @SessionId, @UserKey)", new {SessionId = session.SessionId, UserKey = session.UserKey}
    );

    return Results.Ok(new {SessionId = session.SessionId});

} );

app.Run();