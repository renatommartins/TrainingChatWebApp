using TrainingChatWebApp;
using Dapper;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/hello-world", () => "Hello World!");
app.MapPost("/login", ([FromBody]LoginDto login) =>
{
	var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
	var user = connection.Query<User>(
		"""
			SELECT *
			FROM TrainingChatApp.Users
			WHERE Username = @username
			LIMIT 1
		""", 
		new {username = login.Username}).FirstOrDefault();

	if (user is null)
	{
		return Results.Unauthorized(); //401
	}

	var session = new Session
	{
		UserKey = user.Key,
		SessionId = Guid.NewGuid(),
	};
	
	connection.Execute(
		"""
			INSERT INTO TrainingChatApp.Sessions
			VALUES (NULL, @UserKey, @SessionID)
		""",
		new { UserKey = session.UserKey, SessionId = session.SessionId});

	return Results.Ok(new { SessionId = session.SessionId }); //200
});

app.Run();
