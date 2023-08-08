using TestCliClient;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using TrainingChatApp.Models.Chat;

namespace TestCliClient;

class Program
{
	private static JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
	{
		Converters = {new JsonStringEnumConverter()},
	};
	
	private static async Task Main(string[] args)
	{
		var serverAddress = args[0];
		if (serverAddress[^1] != '/')
			serverAddress += '/';

		var sessionToken = await Login($"http://{serverAddress}", args[1], args[2]);
		var userName = await UserName($"http://{serverAddress}", sessionToken);

		var customHeaders = new List<KeyValuePair<string, string>>
		{
			new ("Authorization", $"Bearer {sessionToken}"),
		};

		var client = new Client(
			$"ws://{serverAddress}ws-test",
			userName,
			customHeaders: customHeaders,
			SendPreprocessor: SendPreprocessor,
			ReceivePreprocessor: ReceivePreprocessor);
		client.ClientMain();
	}

	private static async Task<string> Login(string serverAddress, string username, string password)
	{
		using var httpClient = new HttpClient();

		using var loginRequest = new HttpRequestMessage(HttpMethod.Get, $"{serverAddress}login");
		loginRequest.Headers.Authorization = new AuthenticationHeaderValue(
			"Basic",
			Convert.ToBase64String(Encoding.UTF8.GetBytes($"{username}:{password}")));

		using var loginResponse = await httpClient.SendAsync(loginRequest);

		var loginBody = await JsonSerializer.DeserializeAsync<JsonElement>(await loginResponse.Content.ReadAsStreamAsync(), _jsonOptions);
		var sessionToken = loginBody.GetProperty("sessionId").GetString();
			
		return sessionToken;
	}

	private static async Task<string> UserName(string serverAddress, string sessionToken)
	{
		using var httpClient = new HttpClient();
			
		using var getUserRequest = new HttpRequestMessage(HttpMethod.Get, $"{serverAddress}user");
		getUserRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", sessionToken);
			
		using var getUserResponse = await httpClient.SendAsync(getUserRequest);
			
		var user = await JsonSerializer.DeserializeAsync<JsonElement>(await getUserResponse.Content.ReadAsStreamAsync(),_jsonOptions);
		var userName = user.GetProperty("name").GetString();
			
		return userName;
	}

	private static string SendPreprocessor(string toSend, string username)
	{
		if (toSend[0] != '/')
			return JsonSerializer.Serialize(new ClientMessage
			{
				Type = ClientMessage.DataType.SendChatRoomMessage,
				Data = new ClientMessage.SendChatRoomMessage { Message = toSend }
			}, _jsonOptions);

		var commandSplit = toSend.Split(' ');
		var command = commandSplit[0];
		var arguments = commandSplit[1..];
		switch (command)
		{
			case "/create":
				if (arguments.Length < 1) return null;
				return ClientMessage.CreateMessage(new ClientMessage.CreateChatRoom
				{
					Name = arguments[0],
				}).Serialize(_jsonOptions);
			case "/list":
				return ClientMessage.CreateMessage(new ClientMessage.ListChatRoom()).Serialize(_jsonOptions);
			case "/join":
				if (arguments.Length < 1) return null;
				return ClientMessage.CreateMessage(new ClientMessage.JoinChatRoom
				{
					Id = int.Parse(arguments[0]),
				}).Serialize(_jsonOptions);
			case "/leave":
				return ClientMessage.CreateMessage(new ClientMessage.LeaveChatRoom()).Serialize(_jsonOptions);
			default:
				return null;
		}
	}

	private static string? ReceivePreprocessor(string toReceive)
	{
		var serverMessage = JsonSerializer.Deserialize<ServerMessage>(toReceive, _jsonOptions)!;
		if (!serverMessage.IsSuccessful)
		{
			return $"Error: {serverMessage.Error}";
		}
		
		switch (serverMessage.Type)
		{
			case ServerMessage.DataType.ResponseCreateChatRoom:
				var createResponse = serverMessage.Deserialize<ServerMessage.ResponseCreateChatRoom>(_jsonOptions);
				return $"Created chat room with name \"{createResponse.Name}\" and ID: {createResponse.Id}";
			
			case ServerMessage.DataType.ResponseListChatRoom:
				var listResponse = serverMessage.Deserialize<ServerMessage.ResponseListChatRoom>(_jsonOptions);
				var responseBuilder = new StringBuilder();
				foreach (var chatRoom in listResponse.ChatRooms)
				{
					responseBuilder.Append("ID: ");
					responseBuilder.Append(chatRoom.Id);
					responseBuilder.Append(", ");
					responseBuilder.Append(chatRoom.Name);
					responseBuilder.Append("],");
				}
				if(responseBuilder.Length > 0)
					responseBuilder[^1] = ' ';
				
				return responseBuilder.ToString();
			
			case ServerMessage.DataType.ResponseJoinChatRoom:
				var joinResponse = serverMessage.Deserialize<ServerMessage.ResponseJoinChatRoom>(_jsonOptions);
				var joinBuilder = new StringBuilder();
				foreach (var userName in joinResponse.UserNames)
				{
					joinBuilder.Append(userName);
					joinBuilder.Append(", ");
				}
				return $"Joined chat room with name \"{joinResponse.Name}\" and ID: {joinResponse.Id} along with users: {joinBuilder}";
			
			case ServerMessage.DataType.ResponseLeaveChatRoom:
				var leaveResponse = serverMessage.Deserialize<ServerMessage.ResponseLeaveChatRoom>(_jsonOptions);
				return $"Created chat room with name \"{leaveResponse.Name}\" and ID: {leaveResponse.Id}";
			
			case ServerMessage.DataType.ReceiveChatRoomMessage:
				var receiveResponse = serverMessage.Deserialize<ServerMessage.ReceiveChatRoomMessage>(_jsonOptions);
				return $"{receiveResponse.UserName}: {receiveResponse.Message}";
			
			case ServerMessage.DataType.UserJoinedChatRoom:
				var userJoinedResponse = serverMessage.Deserialize<ServerMessage.UserJoinedChatRoom>(_jsonOptions);
				return $"{userJoinedResponse.UserName} joined.";
				
			case ServerMessage.DataType.UserLeftChatRoom:
				var userLeftResponse = serverMessage.Deserialize<ServerMessage.UserLeftChatRoom>(_jsonOptions);
				return $"{userLeftResponse.UserName} left.";
			case ServerMessage.DataType.ResponseSendChatRoomMessage:
				return "Message sent.";
			default: return "";
		}
	}
}
