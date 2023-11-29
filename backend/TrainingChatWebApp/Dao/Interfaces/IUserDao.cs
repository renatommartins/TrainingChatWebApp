using TrainingChatWebApp.Database.Models;

namespace TrainingChatWebApp.Dao.Interfaces;

public interface IUserDao
{
	bool CheckExistsByUsername(string username);
	Task<bool> CheckExistsByUsernameAsync(string username);
	User? GetUserByKey(int key);
	Task<User?> GetUserByKeyAsync(int key);
	User? GetUserByUsername(string username);
	Task<User?> GetUserByUsernameAsync(string username);
	void CreateUser(User user);
	Task CreateUserAsync(User user);
}
