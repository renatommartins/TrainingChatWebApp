using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Endpoints;
using TrainingChatWebApp.Services.Enums;
using TrainingChatWebApp.Utils.OperationResult;

namespace TrainingChatWebApp.Services.Interfaces;

public interface IUserService
{
	Result<User, SignUpError> SignUp(SignupModel signupModel);
	Task<Result<User, SignUpError>> SignUpAsync(SignupModel signupModel);
	Result<User, GetUserError> GetByKey(int key);
	Task<Result<User, GetUserError>> GetByKeyAsync(int key);
	Result<User, GetUserError> GetByUsername(string username);
	Task<Result<User, GetUserError>> GetByUsernameAsync(string username);
}
