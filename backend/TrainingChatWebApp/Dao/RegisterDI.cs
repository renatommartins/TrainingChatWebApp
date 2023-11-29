using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Dao.Interfaces;

namespace TrainingChatWebApp.Dao;

public static class RegisterDI
{
	public static void Register(IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IUserDao, UserDao>();
		serviceCollection.AddScoped<ISessionDao, SessionDao>();
	}
}
