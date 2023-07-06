using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services;

public class AuthenticationResult
{
	public ResultEnum Result { get; init; }
	public User? User { get; init; }

	public void Deconstruct(out ResultEnum result, out User? user)
	{
		result = Result;
		user = User;
	}
}
