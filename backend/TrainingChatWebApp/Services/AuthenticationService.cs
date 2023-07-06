using Dapper;
using MySql.Data.MySqlClient;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services;

public static class AuthenticationService
{
	public static async Task<AuthenticationResult> AuthenticateSession(string authorization)
	{
		var authSplit = authorization.Split(' ');
		if (authSplit.Length != 2)
		{
			return new AuthenticationResult
			{
				Result = ResultEnum.InvalidFormat,
			}; //Results.BadRequest();
		}

		var scheme = authSplit[0];
		if (scheme != "Bearer")
		{
			return new AuthenticationResult
			{
				Result = ResultEnum.InvalidFormat,
			};
		}

		var sessionGuidString = authSplit[1];
		Guid sessionGuid;
		try
		{
			sessionGuid = Guid.Parse(sessionGuidString);
		}
		catch
		{
			return new AuthenticationResult
			{
				Result = ResultEnum.InvalidFormat,
			};
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
			return new AuthenticationResult
			{
				Result = ResultEnum.Unauthorized,
			};
		}

		var user = (await connection.QueryAsync<User>("""
						SELECT u.Key, u.Username, u.Name
						FROM TrainingChatApp.Users u
						WHERE u.Key = @userKey
						LIMIT 1
					""", new {userKey = session.UserKey})).First();
		
		return new AuthenticationResult
		{
			Result = ResultEnum.Authenticated,
			User = user,
		};
	}
}
