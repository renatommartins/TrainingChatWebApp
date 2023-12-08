using System.Data;
using System.Reflection;
using Dapper;
using FluentMigrator.Runner;
using MySql.Data.MySqlClient;

namespace TrainingChatWebApp.Utils;

public class MySqlMigrationManager : IDisposable
{
	private readonly IMigrationRunner _migrationRunner;
	private readonly IDbConnection _masterDbConnection;

	public MySqlMigrationManager(
		IConfiguration configuration,
		IMigrationRunner migrationRunner)
	{
		_migrationRunner = migrationRunner;
		_masterDbConnection = new MySqlConnection(configuration.GetConnectionString("MasterConnection"));
	}

	public async Task MigrateDatabase(string dbName)
	{
		try
		{
			var isDbCreated = (await _masterDbConnection.QueryAsync<string>(
				"""
					SELECT SCHEMA_NAME
					FROM INFORMATION_SCHEMA.SCHEMATA
					WHERE SCHEMA_NAME = @dbName
				""",
				new {dbName = dbName})).Any();

			if (!isDbCreated)
			{
				await _masterDbConnection.ExecuteAsync($"CREATE DATABASE {dbName}");
				
			}
			
			_migrationRunner.ListMigrations();
			_migrationRunner.MigrateUp();
		}
		catch (Exception e)
		{
			Console.WriteLine(e);
			throw;
		}
	}

	public void Dispose()
	{
		_masterDbConnection.Dispose();
	}
}
