using Konscious.Security.Cryptography;
using TrainingChatWebApp.Utils.Interfaces;

namespace TrainingChatWebApp.Utils;

public class Argon2PasswordHasher : IPasswordHasher
{
	public byte[] Hash(byte[] passwordBuffer, byte[] salt)
	{
		var hasher = new Argon2id(passwordBuffer);
		hasher.DegreeOfParallelism = 8;
		hasher.MemorySize = 16 * 1024;
		hasher.Iterations = 10;
		hasher.Salt = salt;

		var hash = hasher.GetBytes(16);
		return hash;
	}
}
