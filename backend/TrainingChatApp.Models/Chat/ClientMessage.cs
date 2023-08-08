using System.Text;
using System.Text.Json;

namespace TrainingChatApp.Models.Chat;

public class ClientMessage
{
	public DataType Type { get; set; }
	public object Data { get; set; }
	
	public static ClientMessage CreateMessage<T>(T dataObject)
		where T : ClientData, new()
	{
		var dataType = dataObject switch
		{
			CreateChatRoom => DataType.CreateChatRoom,
			ListChatRoom => DataType.ListChatRoom,
			JoinChatRoom => DataType.JoinChatRoom,
			LeaveChatRoom => DataType.LeaveChatRoom,
			SendChatRoomMessage => DataType.SendChatRoomMessage,
			_ => throw new Exception(),
		};
		
		return new ClientMessage
		{
			Type = dataType,
			Data = dataObject,
		};
	}
	
	public enum DataType
	{
		CreateChatRoom,
		ListChatRoom,
		JoinChatRoom,
		LeaveChatRoom,
		SendChatRoomMessage,
	}
	
	public string Serialize(JsonSerializerOptions? options = null) =>
		JsonSerializer.Serialize(this, options);

	public byte[] SerializeToBytes(JsonSerializerOptions? options = null) =>
		Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, options));

	public T? Deserialize<T>(JsonSerializerOptions? options = null) where T : ClientData, new() =>
		((JsonElement) Data).Deserialize<T>(options);

	public abstract class ClientData{}

	public class CreateChatRoom : ClientData
	{
		public string Name { get; set; }
	}
	
	public class ListChatRoom  : ClientData {}
	
	public class JoinChatRoom  : ClientData
	{
		public int Id { get; set; }
	}
	
	public class LeaveChatRoom  : ClientData {}
	
	public class SendChatRoomMessage  : ClientData
	{
		public string Message { get; set; }
	}
}
