namespace TrainingChatWebApp;

internal static class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var app = builder.Build();

		app.UseWebSockets();

		app.MapGet("/hello-world", () => "Hello World!");

		UserEndpoints.MapEndpoints(app);
		ChatEndpoints.MapEndpoints(app);

		app.Run();
	}
}
