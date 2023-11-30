namespace TrainingChatWebApp.Services.Errors;

public enum LogoutError
{
	Unauthorized,
	BadFormat,
	CouldNotConnectToDatabase,
	RaceCondition,
	NotFound
}
