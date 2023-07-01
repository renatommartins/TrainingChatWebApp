using Dapper;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Utils;

namespace TrainingChatWebApp;

public static class UserEndpoints
{
	public static void MapEndpoints(WebApplication app)
	{
		app.MapGet("/login", LoginEndpoint);
		app.MapGet("/user", GetUser);
	}

	private static IResult LoginEndpoint([FromHeader(Name = "Authorization")] string authorization)
	{
		var authorizationBytes = new byte[256];

		var whiteSpaceIndex = authorization.IndexOf(' ') + 1;
		var credentials = new byte[authorization.Length - whiteSpaceIndex];
		for (var i = 0; i < authorization.Length - whiteSpaceIndex; i++)
		{
			credentials[i] = (byte)authorization[i + whiteSpaceIndex];
		}

		var credentialsSpan = new ReadOnlySpan<byte>(credentials);
		var decodedSpan = new Span<byte>(authorizationBytes);
		Base64.FromBase64(credentialsSpan, decodedSpan);
		Array.Fill(credentials, (byte) 0);

		var colonIndex = 0;
		for (var i = 0; i < authorizationBytes.Length; i++)
		{
			if (authorizationBytes[i] != ':') continue;
			colonIndex = i;
			break;
		}

		var actualLength = 0;
		for (var i = 0; i < authorizationBytes.Length; i++)
		{
			if(authorizationBytes[i] != 0) continue;
			actualLength = i;
			break;
		}

		var usernameSpan = new ReadOnlySpan<byte>(authorizationBytes, 0,colonIndex);
		var passwordSpan = new ReadOnlySpan<byte>(authorizationBytes, colonIndex+1, actualLength - (colonIndex + 1));

		var passwordBuffer = new byte[128];
		passwordSpan.CopyTo(passwordBuffer);

		var username = Encoding.UTF8.GetString(usernameSpan);
		
		Array.Fill(authorizationBytes, (byte)0);

		unsafe
		{
			fixed (char* authPointer = authorization)
				for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
					*currentChar = '\0';
		}

		using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
		var user = (connection.Query<User>("""
					SELECT *
					FROM TrainingChatApp.Users
					WHERE Username = @username
					LIMIT 1
				""", new {username = username})).FirstOrDefault();

		if (user is null)
		{
			return Results.Unauthorized();
		}

		var hasher = new Argon2id(passwordBuffer);
		hasher.DegreeOfParallelism = 8;
		hasher.MemorySize = 16 * 1024;
		hasher.Iterations = 10;
		hasher.Salt = user.Salt;

		var hash = hasher.GetBytes(16);

		Array.Fill(passwordBuffer, (byte) 0, 0, passwordBuffer.Length);

		for (var i = 0; i < 16; i++)
		{
			if (hash[i] == user.PasswordHash[i]) continue;
			return Results.Unauthorized();
		}

		var session = new Session {UserKey = user.Key, SessionId = Guid.NewGuid(),};

		connection.Execute("""
					INSERT INTO TrainingChatApp.Sessions (UserKey, SessionId)
					VALUES (@UserKey, @SessionID)
				""", new {UserKey = session.UserKey, SessionId = session.SessionId});

		return Results.Ok(new {SessionId = session.SessionId}); //200
	}
	
	private static async Task<IResult> GetUser([FromHeader(Name = "Authorization")] string authorization)
	{
		var authSplit = authorization.Split(' ');
		if (authSplit.Length != 2)
		{
			return Results.BadRequest();
		}

		var scheme = authSplit[0];
		if (scheme != "Bearer")
		{
			return Results.BadRequest();
		}

		var sessionGuidString = authSplit[1];
		Guid sessionGuid;
		try
		{
			sessionGuid = Guid.Parse(sessionGuidString);
		}
		catch
		{
			return Results.BadRequest();
		}

		await using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
		var session = (await connection.QueryAsync<Session>("""
					SELECT s.Key, s.UserKey
					FROM TrainingChatApp.Sessions s
					WHERE SessionId = @session
					LIMIT 1
				""", new {session = sessionGuid.ToString()})).FirstOrDefault();

		if (session == null)
		{
			return Results.Unauthorized();
		}

		var user = (await connection.QueryAsync<User>("""
						SELECT u.Key, u.Username, u.Name
						FROM TrainingChatApp.Users u
						WHERE u.Key = @userKey
						LIMIT 1
					""", new {userKey = session.UserKey})).First();

		return Results.Ok(new {user.Key, user.Username, user.Name});
	}
}
