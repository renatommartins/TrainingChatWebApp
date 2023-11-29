using Dapper;
using MySql.Data.MySqlClient;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Database.Models;

namespace TrainingChatWebApp.Dao;

public class UserDao : IUserDao
{
	private readonly MySqlConnection _dbConnection;

	public UserDao(MySqlConnection dbConnection)
	{
		_dbConnection = dbConnection;
	}

	public bool CheckExistsByUsername(string username) =>
		Task.Run(async () => await CheckExistsByUsernameAsync(username)).Result;

	public async Task<bool> CheckExistsByUsernameAsync(string username) =>
		(await _dbConnection.QueryAsync<bool>("""
				SELECT u.Key
				FROM TrainingChatApp.Users u
				WHERE u.Username = @username
				LIMIT 1
			""", new {username = username})).Any();

	public User? GetUserByKey(int key) =>
		Task.Run(async () => await GetUserByKeyAsync(key)).Result;

	public async Task<User?> GetUserByKeyAsync(int key) =>
		(await _dbConnection.QueryAsync<User>("""
				SELECT *
				FROM TrainingChatApp.Users u
				WHERE u.Key = @key
				LIMIT 1
			""", new {key = key})).FirstOrDefault();

	public User? GetUserByUsername(string username) =>
		Task.Run(async () => await GetUserByUsernameAsync(username)).Result;

	public async Task<User?> GetUserByUsernameAsync(string username) =>
		(await _dbConnection.QueryAsync<User>("""
					SELECT *
					FROM TrainingChatApp.Users
					WHERE Username = @username
					LIMIT 1
				""", new {username = username})).FirstOrDefault();

	public void CreateUser(User user) =>
		Task.Run(async () => await CreateUserAsync(user)).RunSynchronously();

	public async Task CreateUserAsync(User user) =>
		await _dbConnection.ExecuteAsync("""
				INSERT INTO TrainingChatApp.Users (Username, Name, PasswordHash, Salt)
				VALUES (@Username, @Name, @Password, @Salt)
			""",
			new
			{
				Username = user.Username,
				Name = user.Name,
				Password = user.PasswordHash,
				Salt = user.Salt
			});
}
