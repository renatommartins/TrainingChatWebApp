using System.Text;
using System.Text.Json;

namespace TrainingChatApp.Models.Chat;

public class ServerMessage
{
	public DataType Type { get; set; }
	public object? Data { get; set; }
	public bool IsSuccessful { get; set; }
	public string? Error { get; set; }

	public static ServerMessage CreateMessage<T>(T dataObject)
		where T : ServerData, new()
	{
		var dataType = dataObject switch
		{
			ResponseCreateChatRoom => DataType.ResponseCreateChatRoom,
			ResponseListChatRoom => DataType.ResponseListChatRoom,
			ResponseJoinChatRoom => DataType.ResponseJoinChatRoom,
			ResponseLeaveChatRoom => DataType.ResponseLeaveChatRoom,
			ResponseSendChatRoomMessage => DataType.ResponseSendChatRoomMessage,
			ReceiveChatRoomMessage => DataType.ReceiveChatRoomMessage,
			UserJoinedChatRoom => DataType.UserJoinedChatRoom,
			UserLeftChatRoom => DataType.UserLeftChatRoom,
			_ => throw new Exception(),
		};
		
		return new ServerMessage
		{
			Type = dataType,
			Data = dataObject,
			IsSuccessful = true,
			Error = null,
		};
	}

	public static ServerMessage CreateErrorMessage<T>(string errorMessage)
		where T : ServerData, new()
	{
		var returnValue = new T();
		var dataType = returnValue switch
		{
			ResponseCreateChatRoom => DataType.ResponseCreateChatRoom,
			ResponseListChatRoom => DataType.ResponseListChatRoom,
			ResponseJoinChatRoom => DataType.ResponseJoinChatRoom,
			ResponseLeaveChatRoom => DataType.ResponseLeaveChatRoom,
			ResponseSendChatRoomMessage => DataType.ResponseSendChatRoomMessage,
			ReceiveChatRoomMessage => DataType.ReceiveChatRoomMessage,
			UserJoinedChatRoom => DataType.UserJoinedChatRoom,
			UserLeftChatRoom => DataType.UserLeftChatRoom,
			_ => throw new Exception(),
		};
		
		return new ServerMessage
		{
			Type = dataType,
			IsSuccessful = false,
			Error = errorMessage,
		};
	}

	public string Serialize(JsonSerializerOptions? options = null) =>
		JsonSerializer.Serialize(this, options);

	public byte[] SerializeToBytes(JsonSerializerOptions? options = null) =>
		Encoding.UTF8.GetBytes(JsonSerializer.Serialize(this, options));
	
	public T? Deserialize<T>(JsonSerializerOptions? options = null) where T : ServerData, new() =>
		((JsonElement) Data).Deserialize<T>(options);

	public abstract class ServerData{}

	public enum DataType
	{
		ResponseCreateChatRoom,
		ResponseListChatRoom,
		ResponseJoinChatRoom,
		ResponseLeaveChatRoom,
		ReceiveChatRoomMessage,
		ResponseSendChatRoomMessage,
		UserJoinedChatRoom,
		UserLeftChatRoom,
	}
	
	public class UserLeftChatRoom : ServerData
	{
		public string UserName { get; set; }
	}

	public class UserJoinedChatRoom : ServerData
	{
		public string UserName { get; set; }
	}

	public class ReceiveChatRoomMessage : ServerData
	{
		public string UserName { get; set; }
		public string Message { get; set; }
	}

	public class ResponseLeaveChatRoom : ServerData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class ResponseCreateChatRoom : ServerData
	{
		public int Id { get; set; }
		public string Name { get; set; }
	}

	public class ResponseJoinChatRoom : ServerData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public string[] UserNames { get; set; }
	}

	public class ResponseListChatRoom : ServerData
	{
		public ChatRoomListDescription[] ChatRooms { get; set; }
	}

	public class ChatRoomListDescription
	{
		public int Id{ get; set; }
		public string Name { get; set; }
	}

	public class ResponseSendChatRoomMessage : ServerData
	{
	}
}
