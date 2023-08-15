namespace TrainingChatWebApp;

internal static class Program
{
	public const string AllowedOrigins = "allowed_origins";
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name:AllowedOrigins,
				policy =>
				{
					policy.WithOrigins("http://localhost:5140","http://localhost:5500","http://127.0.0.1:5500");
					policy.WithHeaders("Authorization");
				});
		});
		
		var app = builder.Build();

		app.UseWebSockets();

		app.UseCors(AllowedOrigins);

		app.MapGet("/hello-world", () => "Hello World!");

		UserEndpoints.MapEndpoints(app);
		ChatEndpoints.MapEndpoints(app);

		app.Run();
	}
}
