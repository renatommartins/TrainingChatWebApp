namespace TrainingChatWebApp.Database.Models;

public class User
{
	public int Key { get; set; }
	public string Username { get; set; }
	public string Name { get; set; }
	public byte[] PasswordHash { get; set; }
	public byte[] Salt { get; set; }
}
