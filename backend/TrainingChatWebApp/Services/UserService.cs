using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Dao.Interfaces;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Endpoints;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Services.Interfaces;
using TrainingChatWebApp.Utils.Interfaces;
using TrainingChatWebApp.Utils.OperationResult;

using static TrainingChatWebApp.Utils.OperationResult.Helpers;

namespace TrainingChatWebApp.Services;

public class UserService : IUserService
{
	private readonly IPasswordHasher _passwordHasher;
	private readonly IUserDao _userDao;

	public UserService(
		IPasswordHasher passwordHasher,
		IUserDao userDao)
	{
		_passwordHasher = passwordHasher;
		_userDao = userDao;
	}

	public Result<User, GetUserError> GetByKey(int key) =>
		Task.Run(async () => await GetByKeyAsync(key)).Result;

	public async Task<Result<User, GetUserError>> GetByKeyAsync(int key) =>
		await _userDao.GetUserByKeyAsync(key);

	public Result<User, GetUserError> GetByUsername(string username) =>
		Task.Run(async () => await GetByUsernameAsync(username)).Result;

	public async Task<Result<User, GetUserError>> GetByUsernameAsync(string username)
	{
		var getUserResult = await _userDao.GetUserByUsernameAsync(username);

		if (getUserResult.IsError)
		{
			switch (getUserResult.Error)
			{
				case GetUserError.CouldNotConnectToDatabase:
					return Error(GetUserError.CouldNotConnectToDatabase);
				case GetUserError.UserNotFound:
					return Error(GetUserError.UserNotFound);
				default:
					throw new NotImplementedException();
			}
		}

		var user = getUserResult.Value;

		return user;
	}

	public Result<User, SignUpError> SignUp(SignupModel signupModel) =>
		Task.Run(async () => await SignUpAsync(signupModel)).Result;

	public async Task<Result<User, SignUpError>> SignUpAsync(SignupModel signupModel)
	{
		var checkExistsResult = await _userDao.CheckExistsByUsernameAsync(signupModel.Username);

		if (checkExistsResult.IsError)
		{
			switch (checkExistsResult.Error)
			{
				case SqlError.CouldNotConnect:
					return Error(SignUpError.CouldNotConnectToDatabase);
				default:
					throw new NotImplementedException();
			}
		}
		
		if (checkExistsResult.Value)
		{
			unsafe
			{
				fixed (char* authPointer = signupModel.Password)
					for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
						*currentChar = '\0';
			}
			return Error(SignUpError.UserAlreadyExists);
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

		var hash = _passwordHasher.Hash(passwordBuffer, salt);

		Array.Fill(passwordBuffer, (byte)0x00);

		var createResult = await _userDao.CreateUserAsync(
			new User
			{
				Name = signupModel.Name,
				Username = signupModel.Username,
				PasswordHash = hash,
				Salt = salt,
			});

		if (createResult.IsError)
		{
			switch (createResult.Error)
			{
				case CreateUserError.CouldNotConnectToDatabase:
					return Error(SignUpError.CouldNotConnectToDatabase);
				case CreateUserError.DuplicatedUsername:
				default:
					throw new NotImplementedException();
			}
		}

		var user = createResult.Value;

		return user;
	}
}
