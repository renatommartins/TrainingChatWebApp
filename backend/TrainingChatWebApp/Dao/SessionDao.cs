using System.Data;
using Dapper;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Utils.OperationResult;

using static TrainingChatWebApp.Utils.OperationResult.Helpers;

namespace TrainingChatWebApp.Dao;

public class SessionDao : ISessionDao
{
	private readonly IDbConnection _dbConnection;

	public SessionDao(IDbConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	public Result<Session, CreateSessionError> Create(Session session) =>
		Task.Run(async () => await CreateAsync(session)).Result;

	public async Task<Result<Session, CreateSessionError>> CreateAsync(Session session)
	{
		int key;
		try
		{
			key = (await _dbConnection.QueryAsync<int>(
					"""
						INSERT INTO Sessions (UserKey, SessionId, ExpiresAt)
						VALUES (@UserKey, @SessionID, @ExpiresAt);
						SELECT LAST_INSERT_ID();
					""",
					new
					{
						UserKey = session.UserKey,
						SessionId = session.SessionId,
						ExpiresAt = session.ExpiresAt,
					}))
				.First();
		}
		catch (Exception exception)
		{
			return Error(CreateSessionError.CouldNotConnectToDatabase);
		}
		
		session.Key = key;

		return session;
	}

	public Result<Session, GetSessionError> GetByGuid(Guid guid) =>
		Task.Run(async () => await GetByGuidAsync(guid)).Result;

	public async Task<Result<Session, GetSessionError>> GetByGuidAsync(Guid guid)
	{
		Session? session;
		try
		{
			session = (await _dbConnection.QueryAsync<Session>(
					"""
					SELECT s.Key, s.UserKey, s.ExpiresAt, s.IsLoggedOut
					FROM Sessions s
					WHERE SessionId = @session
					LIMIT 1
				""",
					new
					{
						session = guid.ToString(),
					}))
				.FirstOrDefault();
		}
		catch (Exception exception)
		{
			return Error(GetSessionError.CouldNotConnectToDatabase);
		}
		
		if(session is null)
			return Error(GetSessionError.NotFound);

		return session;
	}

	public Result<Session, UpdateSessionError> Update(Session session) =>
		Task.Run(async () => await UpdateAsync(session)).Result;

	public async Task<Result<Session, UpdateSessionError>> UpdateAsync(Session session)
	{
		try
		{
			var rows = await _dbConnection.ExecuteAsync(
				"""
					UPDATE Sessions s
					SET s.IsLoggedOut = @isLoggedOut
					WHERE s.Key = @key
				""",
				new
				{
					key = session.Key,
					isLoggedOut = session.IsLoggedOut,
				});

			if (rows == 0)
				return Error(UpdateSessionError.NotFound);
		}
		catch (Exception exception)
		{
			return Error(UpdateSessionError.CouldNotConnectToDatabase);
		}

		return session;
	}
}
