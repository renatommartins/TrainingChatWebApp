using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Services.Errors;
using TrainingChatWebApp.Services.Interfaces;
using TrainingChatWebApp.Utils.OperationResult;

using static TrainingChatWebApp.Utils.OperationResult.Helpers;

namespace TrainingChatWebApp.Services;

public class SessionService : ISessionService
{
	private readonly ISessionDao _sessionDao;
	private readonly IUserService _userService;

	public SessionService(
		ISessionDao sessionDao,
		IUserService userService)
	{
		_sessionDao = sessionDao;
		_userService = userService;
	}
	
	public Result<Session, LoginError> Login(string username, byte[] password) =>
		Task.Run(async () => await LoginAsync(username, password)).Result;

	public async Task<Result<Session, LoginError>> LoginAsync(string username, byte[] password)
	{
		var getUserResult = await _userService.GetByUsernameAsync(username);

		if (getUserResult.IsError)
		{
			switch (getUserResult.Error)
			{
				case GetUserError.CouldNotConnectToDatabase:
					return Error(LoginError.CouldNotConnectToDatabase);
				case GetUserError.UserNotFound:
					return Error(LoginError.UserNotRegistered);
				default:
					throw new NotImplementedException();
			}
		}

		var user = getUserResult.Value;
		
		var session = new Session
		{
			UserKey = user.Key,
			SessionId = Guid.NewGuid(),
			ExpiresAt = DateTime.UtcNow.AddHours(16),
		};

		var createResult = await _sessionDao.CreateAsync(session);

		if (createResult.IsError)
		{
			switch (createResult.Error)
			{
				case CreateSessionError.CouldNotConnectToDatabase:
					return Error(LoginError.CouldNotConnectToDatabase);
				default:
					throw new NotImplementedException();
			}
		}

		return createResult.Value;
	}

	public Result<Session, LogoutError> Logout(Guid token) =>
		Task.Run(async () => await LogoutAsync(token)).Result;

	public async Task<Result<Session, LogoutError>> LogoutAsync(Guid token)
	{
		var getResult = await _sessionDao.GetByGuidAsync(token);

		if (getResult.IsError)
		{
			switch (getResult.Error)
			{
				case Dao.Errors.GetSessionError.CouldNotConnectToDatabase:
					return Error(LogoutError.CouldNotConnectToDatabase);
				case Dao.Errors.GetSessionError.NotFound:
					return Error(LogoutError.NotFound);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		var session = getResult.Value;
		session.IsLoggedOut = true;
		var updateResult = await _sessionDao.UpdateAsync(session);

		if (updateResult.IsError)
		{
			switch (updateResult.Error)
			{
				case UpdateSessionError.CouldNotConnectToDatabase:
					return Error(LogoutError.CouldNotConnectToDatabase);
				case UpdateSessionError.NotFound:
					return Error(LogoutError.RaceCondition);
				default:
					throw new NotImplementedException();
			}
		}

		return session;
	}

	public Result<Session, SessionValidationError> ValidateById(Guid sessionGuid) =>
		Task.Run(async () => await ValidateByIdAsync(sessionGuid)).Result;

	public async Task<Result<Session, Services.Errors.SessionValidationError>> ValidateByIdAsync(Guid sessionGuid)
	{
		var sessionResult = await _sessionDao.GetByGuidAsync(sessionGuid);

		if (sessionResult.IsError)
		{
			switch (sessionResult.Error)
			{
				case GetSessionError.CouldNotConnectToDatabase:
					return Error(SessionValidationError.CouldNotConnectToDatabase);
				case GetSessionError.NotFound:
					return Error(SessionValidationError.Unauthorized);
				default:
					throw new NotImplementedException();
			}
		}

		var session = sessionResult.Value;
		if (session.IsLoggedOut || session.ExpiresAt < DateTime.UtcNow)
			return Error(SessionValidationError.Unauthorized);

		return session;
	}
}
