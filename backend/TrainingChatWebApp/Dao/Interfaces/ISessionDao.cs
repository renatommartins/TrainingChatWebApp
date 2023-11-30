using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Utils.OperationResult;

namespace TrainingChatWebApp.Dao.Interfaces;

public interface ISessionDao
{
	Result<Session, CreateSessionError> Create(Session session);
	Task<Result<Session, CreateSessionError>> CreateAsync(Session session);
	Result<Session, GetSessionError> GetByGuid(Guid guid);
	Task<Result<Session, GetSessionError>> GetByGuidAsync(Guid guid);
	Result<Session, UpdateSessionError> Update(Session session);
	Task<Result<Session, UpdateSessionError>> UpdateAsync(Session session);
}
