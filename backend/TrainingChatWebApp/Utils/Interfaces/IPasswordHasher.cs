namespace TrainingChatWebApp.Utils.Interfaces;

public interface IPasswordHasher
{
	byte[] Hash(byte[] passwordBuffer, byte[] salt);
}
