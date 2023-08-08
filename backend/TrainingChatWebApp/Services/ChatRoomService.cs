using System.Collections.ObjectModel;
using TrainingChatApp.Models.Chat;
using TrainingChatWebApp.Database.Models;

namespace TrainingChatWebApp.Services;

public class ChatRoom
{
	private static readonly Dictionary<int, ChatRoom> ChatRoomsBackingField = new();

	private static ReadOnlyDictionary<int, ChatRoom>? _readOnlyChatRooms;

	public static ReadOnlyDictionary<int, ChatRoom> ChatRooms =>
		_readOnlyChatRooms ??= new ReadOnlyDictionary<int, ChatRoom>(ChatRoomsBackingField);

	public static ChatRoom CreateChatRoom(UserConnection userConnection, string name)
	{
		var id = 0;
		for(var i = 1; i < int.MaxValue; i++)
			if (!ChatRoomsBackingField.ContainsKey(i))
			{
				id = i;
				break;
			}

		var newChatRoom = new ChatRoom
		{
			Id = id,
			Name = name,
			_user = new Dictionary<int, UserConnection>
			{
				{userConnection.User.Key, userConnection},
			},
		};
		ChatRoomsBackingField.Add(id, newChatRoom);

		return newChatRoom;
	}

	public static ChatRoom? GetUserChatRoom(User user)
	{
		return ChatRooms
			.Where(p =>
				p.Value.Users
					.Any(pu => pu.Key == user.Key))
			.Select(p => p.Value)
			.FirstOrDefault();
	}

	public static ChatRoom? GetChatRoom(int chatRoomId)
	{
		return ChatRooms
			.Where(p => p.Key == chatRoomId)
			.Select(p => p.Value)
			.FirstOrDefault();
	}

	public static ChatRoom? GetChatRoom(string chatRoomName)
	{
		return ChatRooms
			.Where(p => p.Value.Name == chatRoomName)
			.Select(p => p.Value)
			.FirstOrDefault();
	}

	public required int Id { get; init; }
	public required string Name { get; init; }

	private ChatRoom(){}
	
	private Dictionary<int, UserConnection>? _user;
	private ReadOnlyDictionary<int, UserConnection>? _readOnlyUsers;
	public ReadOnlyDictionary<int, UserConnection> Users =>
		_readOnlyUsers ??= new ReadOnlyDictionary<int, UserConnection>(_user!);
	
	public void SendMessage(User sender, string message)
	{
		foreach (var (_, connection) in _user!)
		{
			connection.Send(ServerMessage.CreateMessage(new ServerMessage.ReceiveChatRoomMessage
			{
				UserName = sender.Name,
				Message = message,
			}).SerializeToBytes());
		}
	}

	public void JoinUser(UserConnection newConnection)
	{
		foreach (var (_, connection) in _user!)
		{
			connection.Send(
				ServerMessage.CreateMessage(
					new ServerMessage.UserJoinedChatRoom
					{
						UserName = newConnection.User.Name,
					}).SerializeToBytes());
		}
		
		_user.Add(newConnection.User.Key, newConnection);
	}

	public void LeaveChatRoom(UserConnection leavingConnection)
	{
		_user!.Remove(leavingConnection.User.Key);

		foreach (var (_, connection) in _user)
		{
			connection.Send(
				ServerMessage.CreateMessage(
					new ServerMessage.UserLeftChatRoom
					{
						UserName = leavingConnection.User.Name,
					}).SerializeToBytes());
		}

		if (_user.Count == 0)
			ChatRoomsBackingField.Remove(Id);
	}
}
