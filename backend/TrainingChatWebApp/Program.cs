using System.Text;
using TrainingChatWebApp;
using Dapper;
using MySql.Data.MySqlClient;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;

//[] - Test Branch
static class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var app = builder.Build();

		app.MapGet("/hello-world", () => "Hello World!");
		app.MapPost("/login", async (HttpContext context) =>
		{
			var usernameBuffer = new byte[128];
			var usernameLength = await ReadLineFromStream(context.Request.Body, usernameBuffer);
			
			var passwordBuffer = new byte[128];
			var passwordLength = await ReadLineFromStream(context.Request.Body, passwordBuffer);

			var username = Encoding.UTF8.GetString(usernameBuffer, 0, usernameLength);

			await using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Port=3307; Database=TrainingChatApp");
			var user = (await connection.QueryAsync<User>(
				"""
					SELECT *
					FROM TrainingChatApp.Users
					WHERE Username = @username
					LIMIT 1
				""",
				new {username = username})).FirstOrDefault();

			if (user is null) { return Results.Unauthorized(); }

			var hasher = new Argon2id(passwordBuffer);
			hasher.DegreeOfParallelism = 8;
			hasher.MemorySize = 16 * 1024;
			hasher.Iterations = 10;
			hasher.Salt = user.Salt;

			var hash = await hasher.GetBytesAsync(16);

			Array.Fill(passwordBuffer, (byte)0, 0, passwordBuffer.Length);
			
			for (var i = 0; i < 16; i++)
			{
				if (hash[i] == user.PasswordHash[i]) continue;
				return Results.Unauthorized();
			}

			var session = new Session
			{
				UserKey = user.Key,
				SessionId = Guid.NewGuid(),
			};
	
			await connection.ExecuteAsync(
				"""
					INSERT INTO TrainingChatApp.Session
					VALUES (NULL, @SessionID, @UserKey)
				""",
				new { UserKey = session.UserKey, SessionId = session.SessionId});

			return Results.Ok(new { SessionId = session.SessionId }); //200
		});
		app.MapGet("/user", async ([FromHeader(Name = "Authorization")] string authorization) => {

			var authorizationSplit = authorization.Split(' ');
			
			if(authorizationSplit.Length != 2)
			{
				return Results.BadRequest();
			}

			var authScheme = authorizationSplit[0];

			if(authScheme != "Bearer")
			{
				return Results.BadRequest();
			}

			var authSessionGuid = authorizationSplit[1];
			try
			{
				_= Guid.Parse(authSessionGuid);
			}
			catch (System.Exception)
			{
				return Results.BadRequest();
			}

			var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Port=3307; Database=TrainingChatApp");
			var session = (await connection.QueryAsync<Session>(
				"""
					SELECT session.Key, session.UserKey
					FROM TrainingChatApp.Session session
					WHERE SessionId = @session
					LIMIT 1
				""",
				new {session = authSessionGuid}
			)).FirstOrDefault();

			if(session == null)
			{
				return Results.Unauthorized();
			}

			var user = (await connection.QueryAsync<User>(
				"""
					SELECT user.Key, user.Username, user.Name
					FROM TrainingChatApp.Users user
					WHERE user.Key = @userKey
					LIMIT 1
				""",
				new {userKey = session.UserKey}
			)).First();

			return Results.Ok(new {user.Key, user.Name, user.Username});
		});

		app.Run();
	}


	
	private static async Task<int> ReadLineFromStream (Stream bodyStream, byte[] buffer)
	{
		var isEof = false;
		var length = 0;
		var current = 0;
		var readByte = new byte[1];
		byte nextByte = 0;

		while (!isEof)
		{
			isEof = await bodyStream.ReadAtLeastAsync(readByte, 1, false) < 0;
			nextByte = readByte[0];
			if (nextByte != '\n' && nextByte != '\r') { break; }
		}
		
		while (!isEof)
		{
			nextByte = readByte[0];
			if (nextByte == '\n' || nextByte == '\r') { break; }
			buffer[current++] = nextByte;
			length++;
			isEof = await bodyStream.ReadAtLeastAsync(readByte, 1, false) < 0;
		}

		readByte[0] = 0;
		nextByte = 0;

		return length;
	}
}
