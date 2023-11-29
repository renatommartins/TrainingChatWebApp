using TrainingChatApp.Models.Database;

namespace TrainingChatWebApp.Dao.Interfaces;

public interface ISessionDao
{
	void Create(Session session);
	Task CreateAsync(Session session);
	Session? GetByGuid(Guid guid);
	Task<Session?> GetByGuidAsync(Guid guid);
	void Update(Session session);
	Task UpdateAsync(Session session);
}
