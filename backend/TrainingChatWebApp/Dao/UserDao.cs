using Dapper;
using MySql.Data.MySqlClient;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Utils.OperationResult;

using static TrainingChatWebApp.Utils.OperationResult.Helpers;

namespace TrainingChatWebApp.Dao;

public class UserDao : IUserDao
{
	private readonly MySqlConnection _dbConnection;

	public UserDao(MySqlConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	public Result<bool, SqlError> CheckExistsByUsername(string username) =>
		Task.Run(async () => await CheckExistsByUsernameAsync(username)).Result;

	public async Task<Result<bool, SqlError>> CheckExistsByUsernameAsync(string username)
	{
		var exists = false;
		try
		{
			exists = (await _dbConnection.QueryAsync<bool>(
					"""
						SELECT u.Key
						FROM TrainingChatApp.Users u
						WHERE u.Username = @username
						LIMIT 1
					""",
					new
					{
						username = username
					}))
				.Any();
		}
		catch (Exception exception)
		{
			return Error(SqlError.CouldNotConnect);
		}

		return exists;
	}

	public Result<User, GetUserError> GetUserByKey(int key) =>
		Task.Run(async () => await GetUserByKeyAsync(key)).Result;

	public async Task<Result<User, GetUserError>> GetUserByKeyAsync(int key)
	{
		User? user = null;
		try
		{
			user = (await _dbConnection.QueryAsync<User>(
					"""
						SELECT *
						FROM TrainingChatApp.Users u
						WHERE u.Key = @key
						LIMIT 1
					""",
					new
					{
						key = key
					}))
				.FirstOrDefault();
		}
		catch (Exception exception)
		{
			return Error(GetUserError.CouldNotConnectToDatabase);
		}
		
		if (user is null)
			return Error(GetUserError.UserNotFound);

		return user;
	}

	public Result<User, GetUserError> GetUserByUsername(string username) =>
		Task.Run(async () => await GetUserByUsernameAsync(username)).Result;

	public async Task<Result<User, GetUserError>> GetUserByUsernameAsync(string username)
	{
		User? user = null;
		try
		{
			user = (await _dbConnection.QueryAsync<User>(
					"""
						SELECT *
						FROM TrainingChatApp.Users
						WHERE Username = @username
						LIMIT 1
					""",
					new
					{
						username = username
					}))
				.FirstOrDefault();
		}
		catch (Exception exception)
		{
			return Error(GetUserError.CouldNotConnectToDatabase);
		}
		
		if (user is null)
			return Error(GetUserError.UserNotFound);

		return user;
	}

	public Result<User, CreateUserError> CreateUser(User user) =>
		Task.Run(async () => await CreateUserAsync(user)).Result;

	public async Task<Result<User, CreateUserError>> CreateUserAsync(User user)
	{
		try
		{
			var userExists = (await _dbConnection.QueryAsync<int>(
				"""
						SELECT COUNT(u.Key)
						FROM TrainingChatApp.Users u
						WHERE u.Username = @username
					""",
				new { username = user.Username }))
				.First();

			if (userExists != 0)
			{
				return Error(CreateUserError.DuplicatedUsername);
			}
			
			user.Key = (await _dbConnection.QueryAsync<int>(
					"""
						INSERT INTO TrainingChatApp.Users (Username, Name, PasswordHash, Salt)
						VALUES (@Username, @Name, @Password, @Salt);

						SELECT LAST_INSERT_ID();
					""",
					new
					{
						Username = user.Username,
						Name = user.Name,
						Password = user.PasswordHash,
						Salt = user.Salt
					}))
				.First();
		}
		catch (Exception exception)
		{
			return Error(CreateUserError.CouldNotConnectToDatabase);
		}

		return user;
	}
}
