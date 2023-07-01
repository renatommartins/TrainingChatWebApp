namespace TrainingChatWebApp.Database.Models;

public class Session
{
	public int Key { get; set; }
	public int UserKey { get; set; }
	public Guid SessionId { get; set; }
}
