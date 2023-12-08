using System.Text.Json;
using System.Text.Json.Serialization;
using TrainingChatApp.Models.Chat;
using TrainingChatWebApp.Dao.Errors;
using TrainingChatWebApp.Services;
using TrainingChatWebApp.Services.Errors;
using TrainingChatWebApp.Services.Interfaces;

namespace TrainingChatWebApp.Endpoints;

public static class ChatEndpoints
{
	public static void MapEndpoints(WebApplication app)
	{
		app.MapGet("/chat-ws", ChatWebsocket);
	}
	
	private static async Task ChatWebsocket (
		IUserService userService,
		ISessionService sessionService,
		HttpContext context,
		CancellationToken ct)
	{
		var sessionGuid = Guid.Parse(context.Request.Headers.WebSocketSubProtocols);

		var validateResult = await sessionService.ValidateByIdAsync(sessionGuid);

		if (validateResult.IsError)
		{
			switch (validateResult.Error)
			{
				case SessionValidationError.Unauthorized:
				{
					context.Response.StatusCode = StatusCodes.Status401Unauthorized;
					return;
				}
				case SessionValidationError.CouldNotConnectToDatabase:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var session = validateResult.Value;
		
		var getUserResult = await userService.GetByKeyAsync(session.UserKey);

		if (getUserResult.IsError)
		{
			switch (getUserResult.Error)
			{
				case GetUserError.CouldNotConnectToDatabase:
				case GetUserError.UserNotFound:
					throw new Exception();
				default:
					throw new NotImplementedException();
			}
		}

		var user = getUserResult.Value;

		var jsonOptions = new JsonSerializerOptions
		{
			Converters = {new JsonStringEnumConverter()},
		};

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
							Users = chatRoom.Users
								.Select(u => new ServerMessage.JoinedUsers
								{
									Id = u.Value.User.Key,
									Name = u.Value.User.Name,
								})
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
