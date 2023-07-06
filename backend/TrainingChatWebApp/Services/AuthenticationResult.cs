using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services;

public class AuthenticationResult
{
	public ResultEnum Result { get; init; }
	public User? User { get; init; }
	public Session? Session { get; init; }

	public void Deconstruct(out ResultEnum result, out User? user, out Session? session)
	{
		result = Result;
		user = User;
		session = Session;
	}
}
