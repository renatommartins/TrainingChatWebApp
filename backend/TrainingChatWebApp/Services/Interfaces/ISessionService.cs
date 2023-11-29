using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services.Interfaces;

public interface ISessionService
{
	Session? Login(User user);
	Task<Session?> LoginAsync(User user);
	ResultEnum Logout(Guid token);
	Task<ResultEnum> LogoutAsync(Guid token);
	Session? GetActiveById(Guid sessionGuid);
	Task<Session?> GetActiveByIdAsync(Guid sessionGuid);
}
