using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Services.Errors;
using TrainingChatWebApp.Utils.OperationResult;

namespace TrainingChatWebApp.Services.Interfaces;

public interface ISessionService
{
	Result<Session, LoginError> Login(string username, byte[] password);
	Task<Result<Session, LoginError>> LoginAsync(string username, byte[] password);
	Result<Session, LogoutError> Logout(Guid token);
	Task<Result<Session, LogoutError>> LogoutAsync(Guid token);
	Result<Session, SessionValidationError> ValidateById(Guid sessionGuid);
	Task<Result<Session, SessionValidationError>> ValidateByIdAsync(Guid sessionGuid);
}
