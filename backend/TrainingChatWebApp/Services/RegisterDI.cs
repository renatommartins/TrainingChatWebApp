using TrainingChatWebApp.Services.Interfaces;

namespace TrainingChatWebApp.Services;

public static class RegisterDI
{
	public static void Register(IServiceCollection serviceCollection)
	{
		serviceCollection.AddScoped<IUserService, UserService>();
		serviceCollection.AddScoped<ISessionService, SessionService>();
	}
}
