using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Services.Interfaces;

namespace TrainingChatWebApp.Services;

public class SessionService : ISessionService
{
	private readonly ISessionDao _sessionDao;

	public SessionService(ISessionDao sessionDao)
	{
		_sessionDao = sessionDao;
	}
	
	public Session? Login(User user) =>
		Task.Run(async () => await LoginAsync(user)).Result;

	public async Task<Session?> LoginAsync(User user)
	{
		var session = new Session
		{
			UserKey = user.Key,
			SessionId = Guid.NewGuid(),
			ExpiresAt = DateTime.UtcNow.AddHours(16),
		};

		await _sessionDao.CreateAsync(session);

		return session;
	}

	public ResultEnum Logout(Guid token) =>
		Task.Run(async () => await LogoutAsync(token)).Result;

	public async Task<ResultEnum> LogoutAsync(Guid token)
	{
		var session = await _sessionDao.GetByGuidAsync(token);

		if (session is null)
			return ResultEnum.Unauthorized;

		session.IsLoggedOut = true;
		await _sessionDao.UpdateAsync(session);

		return ResultEnum.Authenticated;
	}

	public Session? GetActiveById(Guid sessionGuid) =>
		Task.Run(async () => await GetActiveByIdAsync(sessionGuid)).Result;

	public async Task<Session?> GetActiveByIdAsync(Guid sessionGuid)
	{
		var session = await _sessionDao.GetByGuidAsync(sessionGuid);
		if (session is null || session.IsLoggedOut)
			return null;

		return session;
	}
}
