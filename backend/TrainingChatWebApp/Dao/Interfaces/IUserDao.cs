using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Utils.OperationResult;

namespace TrainingChatWebApp.Dao.Interfaces;

public interface IUserDao
{
	Result<bool, SqlError> CheckExistsByUsername(string username);
	Task<Result<bool, SqlError>> CheckExistsByUsernameAsync(string username);
	Result<User, GetUserError> GetUserByKey(int key);
	Task<Result<User, GetUserError>> GetUserByKeyAsync(int key);
	Result<User, GetUserError> GetUserByUsername(string username);
	Task<Result<User, GetUserError>> GetUserByUsernameAsync(string username);
	Result<User, CreateUserError> CreateUser(User user);
	Task<Result<User, CreateUserError>> CreateUserAsync(User user);
}
