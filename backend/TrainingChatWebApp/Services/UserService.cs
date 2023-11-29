using Konscious.Security.Cryptography;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Services.Interfaces;

namespace TrainingChatWebApp.Services;

public class UserService : IUserService
{
	private readonly IUserDao _userDao;
	private readonly ISessionService _sessionService;

	public UserService(
		IUserDao userDao,
		ISessionService sessionService)
	{
		_userDao = userDao;
		_sessionService = sessionService;
	}

	public User? GetById(int key) =>
		Task.Run(async () => await GetByKeyAsync(key)).Result;

	public async Task<User?> GetByKeyAsync(int key) =>
		await _userDao.GetUserByKeyAsync(key);

	public Session? Login(string username, byte[] passwordBuffer) =>
		Task.Run(async () => await LoginAsync(username, passwordBuffer)).Result;

	public async Task<Session?> LoginAsync(string username, byte[] passwordBuffer)
	{
		var user = await _userDao.GetUserByUsernameAsync(username);

		if (user is null)
		{
			return null;
		}

		var hash = HashPassword(passwordBuffer, user.Salt);

		Array.Fill(passwordBuffer, (byte)0, 0, passwordBuffer.Length);

		for (var i = 0; i < 16; i++)
		{
			if (hash[i] == user.PasswordHash[i]) continue;
			return null;
		}

		var session = await _sessionService.LoginAsync(user);

		return session; //200
	}

	public ResultEnum Logout(Guid token) =>
		Task.Run(async () => await LogoutAsync(token)).Result;

	public async Task<ResultEnum> LogoutAsync(Guid token)
	{
		var result = await _sessionService.LogoutAsync(token);
			
		switch (result)
		{
			case ResultEnum.InvalidFormat:
			case ResultEnum.Unauthorized:
				return result;
		}
			
		return result;
	}

	public SignUpEnum SignUp(SignupModel signupModel) =>
		Task.Run(async () => await SignUpAsync(signupModel)).Result;

	public async Task<SignUpEnum> SignUpAsync(SignupModel signupModel)
	{
		if (await _userDao.CheckExistsByUsernameAsync(signupModel.Username))
		{
			unsafe
			{
				fixed (char* authPointer = signupModel.Password)
					for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
						*currentChar = '\0';
			}
			return SignUpEnum.UserAlreadyExists;
		}

		var passwordBuffer = new byte[128];
		var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

		for (var i = 0; i < signupModel.Password.Length; i++)
		{
			passwordBuffer[i] = (byte)signupModel.Password[i];
		}

		unsafe
		{
			fixed (char* authPointer = signupModel.Password)
				for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
					*currentChar = '\0';
		}

		var hash = HashPassword(passwordBuffer, salt);

		Array.Fill(passwordBuffer, (byte)0x00);

		await _userDao.CreateUserAsync(
			new User
			{
				Name = signupModel.Name,
				Username = signupModel.Username,
				PasswordHash = hash,
				Salt = salt,
			});

		return SignUpEnum.UserCreated;
	}
		
	private static byte[] HashPassword(byte[] passwordBuffer, byte[] salt)
	{
		var hasher = new Argon2id(passwordBuffer);
		hasher.DegreeOfParallelism = 8;
		hasher.MemorySize = 16 * 1024;
		hasher.Iterations = 10;
		hasher.Salt = salt;

		var hash = hasher.GetBytes(16);
		return hash;
	}
}
