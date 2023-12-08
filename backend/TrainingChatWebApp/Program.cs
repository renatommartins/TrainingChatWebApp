using System.Data;
using System.Reflection;
using FluentMigrator.Runner;
using Microsoft.Extensions.FileProviders;
using MySql.Data.MySqlClient;
using TrainingChatWebApp.Endpoints;
using TrainingChatWebApp.Utils;
using TrainingChatWebApp.Utils.Interfaces;

namespace TrainingChatWebApp;

internal static class Program
{
	public const string AllowedOrigins = "allowed_origins";
	private static async Task Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.Services.AddCors(options =>
		{
			options.AddPolicy(
				name:AllowedOrigins,
				policy =>
				{
					policy.WithOrigins("http://localhost:5140","http://localhost:5500","http://127.0.0.1:5500");
					policy.WithHeaders("Authorization","Content-type");
				});
		});
		
		builder.Services.AddScoped<IDbConnection>((provider) =>
			new MySqlConnection(builder.Configuration.GetConnectionString("SqlConnection")));
		builder.Services.AddScoped<MySqlMigrationManager>();
		builder.Services
			.AddFluentMigratorCore()
			.ConfigureRunner(
				c => c.AddMySql5()
					.WithGlobalConnectionString(builder.Configuration.GetConnectionString("SqlConnection"))
					.ScanIn(Assembly.GetExecutingAssembly()).For.Migrations());
		
		Dao.RegisterDI.Register(builder.Services);
		Services.RegisterDI.Register(builder.Services);
		builder.Services.AddSingleton<IPasswordHasher, Argon2PasswordHasher>();

		var app = builder.Build();

		var migrationScope = app.Services.CreateScope();
		var migrationManager = migrationScope.ServiceProvider.GetRequiredService<MySqlMigrationManager>();
		await migrationManager.MigrateDatabase(app.Configuration["Database:Name"]!);

		var options = new DefaultFilesOptions();
		options.DefaultFileNames.Clear();
		options.DefaultFileNames.Add("wwwroot-nocache/index.html");
		app.UseDefaultFiles(options);
		
		app.UseStaticFiles();
		app.UseStaticFiles(new StaticFileOptions
		{
			FileProvider = new PhysicalFileProvider(
				Path.Combine(builder.Environment.ContentRootPath, "wwwroot-nocache")),

			OnPrepareResponse = ctx =>
			{
				ctx.Context.Response.Headers.Append(
					"Cache-Control", $"no-cache");
			}
		});

		app.UseWebSockets();

		app.UseCors(AllowedOrigins);

		app.MapGet("/hello-world", () => "Hello World!");

		UserEndpoints.MapEndpoints(app);
		ChatEndpoints.MapEndpoints(app);

		app.Run();
	}
}
