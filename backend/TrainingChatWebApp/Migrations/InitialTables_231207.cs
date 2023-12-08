using FluentMigrator;

namespace TrainingChatWebApp.Migrations;

[Migration(231207)]
public class InitialTables_231207 : Migration
{
	public override void Up()
	{
		Create.Table("Users")
			.WithColumn("Key").AsInt32().NotNullable().PrimaryKey().Identity()
			.WithColumn("Username").AsString(25).NotNullable()
			.WithColumn("Name").AsString(200).NotNullable()
			.WithColumn("PasswordHash").AsBinary(16).NotNullable()
			.WithColumn("Salt").AsBinary(32).NotNullable();

		Create.Table("Sessions")
			.WithColumn("Key").AsInt32().NotNullable().PrimaryKey().Identity()
			.WithColumn("SessionId").AsString(36).NotNullable()
			.WithColumn("UserKey").AsInt32().NotNullable().ForeignKey("Users", "Key")
			.WithColumn("ExpiresAt").AsDateTime().NotNullable().WithDefaultValue(new DateTime(2020, 01, 01))
			.WithColumn("IsLoggedOut").AsBoolean().NotNullable().WithDefaultValue(false);
	}

	public override void Down()
	{
		Delete.Table("Sessions");
		Delete.Table("Users");
	}
}
