using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Services.Errors;
using TrainingChatWebApp.Services.Interfaces;
using TrainingChatWebApp.Utils;

namespace TrainingChatWebApp.Endpoints;

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
		IUserService userService,
		HttpContext context,
		[FromBody] SignupModel signupModel)
	{
		var signupResult = userService.SignUp(signupModel);
		if (signupResult.IsError)
		{
			switch (signupResult.Error)
			{
				case SignUpError.UserAlreadyExists:
				case SignUpError.EmailAlreadyRegistered:
					return Results.Problem(detail: "User already exists", statusCode: (int)HttpStatusCode.Conflict);
				case SignUpError.CouldNotConnectToDatabase:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var user = signupResult.Value;
		return Results.Ok(user);
	}

	private static IResult LoginEndpoint(
		IUserService userService,
		ISessionService sessionService,
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
		var loginResult = sessionService.Login(username, passwordBuffer);

		if (loginResult.IsError)
		{
			switch (loginResult.Error)
			{
				case LoginError.Unauthorized:
				case LoginError.UserNotRegistered:
					return Results.Unauthorized();
				case LoginError.CouldNotConnectToDatabase:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var session = loginResult.Value;
		
		return Results.Ok(new { SessionId = session.SessionId });
	}

	private static IResult LogoutEndpoint(
		ISessionService sessionService,
		[FromHeader(Name = "Authorization")] string authorization)
	{
		var authorizationSplit = authorization.Split(' ');

		if (authorizationSplit.Length != 2)
			return Results.BadRequest();
		
		if (authorizationSplit[0] != "Bearer")
			return Results.BadRequest();

		if (!Guid.TryParse(authorizationSplit[1], out var token))
			return Results.BadRequest();
		
		var logoutResult = sessionService.Logout(token);

		if (logoutResult.IsError)
		{
			switch (logoutResult.Error)
			{
				case LogoutError.NotFound:
				case LogoutError.Unauthorized:
					return Results.Unauthorized();
				case LogoutError.BadFormat:
					return Results.BadRequest();
				case LogoutError.CouldNotConnectToDatabase:
				case LogoutError.RaceCondition:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}
		
		return Results.Ok();
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

		var validateResult = sessionService.ValidateById(sessionGuid);

		if (validateResult.IsError)
		{
			switch (validateResult.Error)
			{
				case SessionValidationError.Unauthorized:
					return Results.Unauthorized();
				case SessionValidationError.CouldNotConnectToDatabase:
					throw new  Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var session = validateResult.Value;

		var getUserResult = userService.GetByKey(session.UserKey);

		if (getUserResult.IsError)
		{
			switch (getUserResult.Error)
			{
				case GetUserError.CouldNotConnectToDatabase:
				case GetUserError.UserNotFound:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var user = getUserResult.Value;
		
		return Results.Ok(new { Id = user.Key, user.Username, user.Name });
	}
}

public class SignupModel
{
	public string Username { get; set; }
	public string Password { get; set; }
	public string Name { get; set; }
}
