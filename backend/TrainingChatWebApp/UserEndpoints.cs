using System.Net;
using Dapper;
using Konscious.Security.Cryptography;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using System.Text;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Utils;
using System.Linq.Expressions;
using TrainingChatWebApp.Services.Interfaces;

namespace TrainingChatWebApp;

public static class UserEndpoints
{
	public static void MapEndpoints(WebApplication app)
	{
		app.MapPost("/signup", SignupUser).RequireCors(Program.AllowedOrigins);
		app.MapGet("/login", LoginEndpoint).RequireCors(Program.AllowedOrigins);
		app.MapGet("/logout", LogoutEndpoint).RequireCors(Program.AllowedOrigins);
		app.MapGet("/user", GetUser).RequireCors(Program.AllowedOrigins);
	}

	private static IResult SignupUser(
		/*[FromServices]*/IUserService userService,
		HttpContext context,
		[FromBody] SignupModel signupModel)
	{
		var resultSignUp = userService.SignUp(signupModel);
		switch (resultSignUp)
		{
			case SignUpEnum.UserAlreadyExists:
				return Results.Problem(detail: "User already exists", statusCode: (int)HttpStatusCode.Conflict);
			case SignUpEnum.UserCreated:
				return Results.Ok();
			default: throw new NotImplementedException();
		}
	}

	private static IResult LoginEndpoint(
		IUserService userService,
		[FromHeader(Name = "Authorization")] string authorization)
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
		var session = userService.Login(username, passwordBuffer);
		if (session is null)
		{
			return Results.Unauthorized();
		} else
		{
			return Results.Ok(new { SessionId = session.SessionId });
		}
	}

	private static IResult LogoutEndpoint(
		IUserService userService,
		[FromHeader(Name = "Authorization")] string authorization)
	{
		var result = userService.Logout(Guid.Parse(authorization));
		switch (result)
		{
			case ResultEnum.InvalidFormat:
				return Results.BadRequest();
			case ResultEnum.Unauthorized:
				return Results.Unauthorized();
			case ResultEnum.Authenticated:
				return Results.Ok();
			default: throw new NotImplementedException();
		}
	}

	private static IResult GetUser(
		IUserService userService,
		ISessionService sessionService,
		[FromHeader(Name = "Authorization")] string authorization)
	{
		var authSplit = authorization.Split(' ');
		if (authSplit.Length != 2)
			return Results.BadRequest();

		var scheme = authSplit[0];
		if (scheme != "Bearer")
			return Results.BadRequest();

		var sessionGuidString = authSplit[1];
		Guid sessionGuid;
		try { sessionGuid = Guid.Parse(sessionGuidString); }
		catch { return Results.BadRequest(); }

		var session = sessionService.GetActiveById(sessionGuid);
		
		if(session is null)
			return Results.Unauthorized();

		var resultUser = userService.GetById(session.UserKey);
		return Results.Ok(new { Id = resultUser!.Key, resultUser.Username, resultUser.Name });
	}
}

public class SignupModel
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Name { get; set; }
}
