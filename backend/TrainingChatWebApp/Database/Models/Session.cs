namespace TrainingChatWebApp.Database.Models;

public class Session
{
	public int Key { get; set; }
	public int UserKey { get; set; }
	public Guid SessionId { get; set; }
	public DateTime ExpiresAt { get; set; }
	public bool IsLoggedOut { get; set; }
}
