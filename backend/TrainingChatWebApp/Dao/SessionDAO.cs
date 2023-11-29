using Dapper;
using MySql.Data.MySqlClient;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Interfaces;

namespace TrainingChatWebApp.Dao;

public class SessionDao : ISessionDao
{
	private readonly MySqlConnection _dbConnection;

	public SessionDao(MySqlConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	public void Create(Session session) =>
		Task.Run(async () => await CreateAsync(session)).RunSynchronously();

	public async Task CreateAsync(Session session) =>
		await _dbConnection.ExecuteAsync(
			"""
				INSERT INTO TrainingChatApp.Sessions (UserKey, SessionId, ExpiresAt)
				VALUES (@UserKey, @SessionID, @ExpiresAt)
			""",
			new
			{
				UserKey = session.UserKey,
				SessionId = session.SessionId,
				ExpiresAt = session.ExpiresAt,
			});

	public void LogoutByKey(int key) =>
		Task.Run(async () => await LogoutByKeyAsync(key)).RunSynchronously();

	public async Task LogoutByKeyAsync(int key) =>
		await _dbConnection.ExecuteAsync(
			"""
				UPDATE TrainingChatApp.Sessions s
				SET s.IsLoggedOut = 1
				WHERE s.Key = @Key
			""",
			new
			{
				Key = key,
			});

	public Session? GetByGuid(Guid guid) =>
		Task.Run(async () => await GetByGuidAsync(guid)).Result;

	public async Task<Session?> GetByGuidAsync(Guid guid) =>
		(await _dbConnection.QueryAsync<Session>(
			"""
					SELECT s.Key, s.UserKey, s.ExpiresAt, s.IsLoggedOut
					FROM TrainingChatApp.Sessions s
					WHERE SessionId = @session
					LIMIT 1
				""",
			new
			{
				session = guid.ToString(),
			}))
		.FirstOrDefault();

	public void Update(Session session)
	{
		throw new NotImplementedException();
	}

	public async Task UpdateAsync(Session session)
	{
		await _dbConnection.ExecuteAsync(
		"""
				UPDATE TrainingChatApp.Sessions s
				SET s.IsLoggedOut = @isLoggedOut
				WHERE s.Key = @key
			""",
			new
			{
				key = session.Key,
				isLoggedOut = session.IsLoggedOut,
			});
	}
}
