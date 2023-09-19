using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrainingChatApp.Models.Chat;
using TrainingChatWebApp.Services;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp;

public static class ChatEndpoints
{
	public static void MapEndpoints(WebApplication app)
	{
		app.MapGet("/chat-ws", ChatWebsocket).RequireCors(Program.AllowedOrigins);
	}
	
	private static async Task ChatWebsocket (HttpContext context, CancellationToken ct)
	{
		var authHeader = "Bearer " + context.Request.Headers.WebSocketSubProtocols;
		var (result, user, _) =
			await AuthenticationService.AuthenticateSession(authHeader);

		var jsonOptions = new JsonSerializerOptions
		{
			Converters = {new JsonStringEnumConverter()},
		};

		switch (result)
		{
			case ResultEnum.InvalidFormat:
			{
				context.Response.StatusCode = StatusCodes.Status400BadRequest;
				return;
			}
			case ResultEnum.Unauthorized:
			{
				context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return;
			}
			case ResultEnum.Authenticated:
				break;
			default:
				throw new ArgumentOutOfRangeException();
		}

		if (user is null)
			throw new InvalidProgramException();

		if (!context.WebSockets.IsWebSocketRequest)
		{
			context.Response.StatusCode = StatusCodes.Status204NoContent;
			return;
		}
		
		using var webSocket = await context.WebSockets.AcceptWebSocketAsync(context.Request.Headers.WebSocketSubProtocols);
		
		var userConnection = new UserConnection(user, webSocket);
		_ = userConnection.Start(ct);
		
		UserConnection.UserSessions.Add(user.Key, userConnection);

		try
		{
			while (userConnection.IsConnected)
			{
				if (userConnection.Available == 0)
				{
					await Task.Delay(100, ct);
					continue;
				}

				var receivedBytes = userConnection.Receive()!;

				var clientMessage = JsonSerializer.Deserialize<ClientMessage>(
					new ReadOnlySpan<byte>(receivedBytes),
					jsonOptions);

				if (clientMessage is null)
					continue;

				switch (clientMessage.Type)
				{
					case ClientMessage.DataType.CreateChatRoom:
					{
						var createMessage = clientMessage.Deserialize<ClientMessage.CreateChatRoom>(jsonOptions);
						if (createMessage is null)
							continue;

						if (ChatRoom.GetUserChatRoom(user) is not null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.ResponseCreateChatRoom>(
										"User is already in a Chat Room")
									.SerializeToBytes(jsonOptions));
							break;
						}

						if (ChatRoom.GetChatRoom(createMessage.Name) is not null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.ResponseCreateChatRoom>(
										"There is already a chat room with this name")
									.SerializeToBytes(jsonOptions));
							break;
						}

						var chatRoom = ChatRoom.CreateChatRoom(userConnection, createMessage.Name);

						userConnection.Send(ServerMessage.CreateMessage(
							new ServerMessage.ResponseCreateChatRoom
							{
								Id = chatRoom.Id,
								Name = chatRoom.Name,
							}).SerializeToBytes(jsonOptions));
						break;
					}
					case ClientMessage.DataType.ListChatRoom:
					{
						var listMessage = clientMessage.Deserialize<ClientMessage.ListChatRoom>(jsonOptions);
						if (listMessage is null)
							continue;

						userConnection.Send(
							ServerMessage.CreateMessage(new ServerMessage.ResponseListChatRoom
							{
								ChatRooms = ChatRoom.ChatRooms.Select(p =>
									new ServerMessage.ChatRoomListDescription
									{
										Id = p.Value.Id,
										Name = p.Value.Name,
									}).ToArray()
							}).SerializeToBytes(jsonOptions));
						break;
					}
					case ClientMessage.DataType.JoinChatRoom:
					{
						var joinMessage = clientMessage.Deserialize<ClientMessage.JoinChatRoom>(jsonOptions);
						if (joinMessage is null)
							continue;

						if (ChatRoom.GetUserChatRoom(user) is not null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.UserJoinedChatRoom>(
										"User is already in a Chat Room")
									.SerializeToBytes(jsonOptions));
							break;
						}

						var chatRoom = ChatRoom.GetChatRoom(joinMessage.Id);
						if (chatRoom is null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.ResponseJoinChatRoom>(
										"There is already a chat room with this name")
									.SerializeToBytes(jsonOptions));
							break;
						}

						chatRoom.JoinUser(userConnection);

						userConnection.Send(ServerMessage.CreateMessage(new ServerMessage.ResponseJoinChatRoom
						{
							Id = chatRoom.Id,
							Name = chatRoom.Name,
							UserNames = chatRoom.Users
								.Select(u => u.Value.User.Name)
								.ToArray(),
						}).SerializeToBytes(jsonOptions));
						break;
					}
					case ClientMessage.DataType.LeaveChatRoom:
					{
						var leaveMessage = clientMessage.Deserialize<ClientMessage.LeaveChatRoom>(jsonOptions);
						if (leaveMessage is null)
							continue;

						var chatRoom = ChatRoom.GetUserChatRoom(user);
						if (chatRoom is null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.ResponseLeaveChatRoom>(
										"User is not in a Chat Room")
									.SerializeToBytes(jsonOptions));
							break;
						}

						chatRoom.LeaveChatRoom(userConnection);

						userConnection.Send(ServerMessage.CreateMessage(new ServerMessage.ResponseLeaveChatRoom
						{
							Id = chatRoom.Id,
							Name = chatRoom.Name,
						}).SerializeToBytes(jsonOptions));
						break;
					}
					case ClientMessage.DataType.SendChatRoomMessage:
					{
						var sendChatMessage =
							clientMessage.Deserialize<ClientMessage.SendChatRoomMessage>(jsonOptions);
						if (sendChatMessage is null)
							continue;

						var chatRoom = ChatRoom.GetUserChatRoom(user);
						if (chatRoom is null)
						{
							userConnection.Send(
								ServerMessage
									.CreateErrorMessage<ServerMessage.ResponseCreateChatRoom>(
										"User is not in a Chat Room")
									.SerializeToBytes(jsonOptions));
							break;
						}

						chatRoom.SendMessage(user, sendChatMessage.Message);

						userConnection.Send(ServerMessage
							.CreateMessage(new ServerMessage.ResponseSendChatRoomMessage())
							.SerializeToBytes(jsonOptions));
						break;
					}
					default: throw new InvalidProgramException();
				}
			}
		}
		catch(Exception exception)
		{
			ChatRoom.GetUserChatRoom(userConnection.User)?.LeaveChatRoom(userConnection);
			UserConnection.UserSessions.Remove(userConnection.User.Key);
		}
	}
}
