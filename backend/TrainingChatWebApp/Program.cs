using TrainingChatWebApp;

internal static class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var app = builder.Build();

		app.MapGet("/hello-world", () => "Hello World!");

		UserEndpoints.MapEndpoints(app);

		app.Run();
	}
}
