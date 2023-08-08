using System.Net.WebSockets;
using System.Text;
using System.Text.Json;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services;

public class UserConnection
{
	public static Dictionary<int, UserConnection> UserSessions { get; }

	static UserConnection()
	{
		UserSessions = new Dictionary<int, UserConnection>();
	}

	public static bool SendMessageToUser(int id, byte[] message)
	{
		if (!UserSessions.ContainsKey(id))
			return false;
		
		UserSessions[id].Send(message);
		return true;
	}
	
	private bool isStopRequested;
	private Queue<byte[]> sendQueue;
	private Queue<byte[]> receiveQueue;

	public User User { get; init; }
	public WebSocket WebSocket { get; init; }
	public int Available => receiveQueue.Count;
	public bool IsConnected => WebSocket.State == WebSocketState.Open && !isStopRequested;

	public UserConnection(User user, WebSocket webSocket)
	{
		User = user;
		WebSocket = webSocket;

		isStopRequested = false;
		sendQueue = new Queue<byte[]>();
		receiveQueue = new Queue<byte[]>();
	}

	public async Task Start(CancellationToken ct)
	{
		while (WebSocket.State == WebSocketState.Connecting)
			await Task.Delay(100, ct);

		Task<WebSocketReceiveResult>? receiveTask = default;
		var receiveBuffer = new byte[2048];
		while (WebSocket.State == WebSocketState.Open && !ct.IsCancellationRequested && !isStopRequested)
		{
			if (WebSocket.State is not WebSocketState.Open)
				break;
			
			while (sendQueue.Count > 0)
			{
				var message = sendQueue.Dequeue();

				await WebSocket.SendAsync(
					new ArraySegment<byte>(message),
					WebSocketMessageType.Text, 
					true,
					ct);
			}

			if (receiveTask is null)
			{
				receiveTask = WebSocket.ReceiveAsync(new ArraySegment<byte>(receiveBuffer), ct);
				continue;
			}

			if(!receiveTask!.IsCompleted)
			{
				await Task.Delay(100, ct);
				continue;
			}

			var resultWs = receiveTask.Result;
			receiveTask = null;
			
			if(resultWs.MessageType is WebSocketMessageType.Close or not WebSocketMessageType.Text)
				break;

			var receivedBytes = new byte[resultWs.Count];
			Array.Copy(receiveBuffer, 0, receivedBytes, 0, resultWs.Count);
			
			receiveQueue.Enqueue(receivedBytes);
		}

		isStopRequested = true;
	}

	public void Stop() => isStopRequested = true;

	public void Send(byte[] message) => sendQueue.Enqueue(message);

	public byte[]? Receive() => receiveQueue.Count > 0 ? receiveQueue.Dequeue() : null;
}
