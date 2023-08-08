using System.Text;

namespace TestCliClient;

class Client
{
	private readonly string _userName;
	private readonly Func<string, string, string?>? _sendPreprocessor;
	private readonly Func<string, string?>? _receivePreprocessor;
		
	string _clientName;
		
	WebSocket4Net.WebSocket _client;

	ReceiveProxy _receiveProxy;
		
	private StringBuilder _writeContent;
		
	private List<(string text, DateTime timestamp, int rtt, bool sent)> _messageLog;

	public Client(
		string uri,
		string userName,
		List<KeyValuePair<string,string>>? customHeaders = default,
		Func<string,string, string?>? SendPreprocessor = default,
		Func<string, string?>? ReceivePreprocessor = default)
	{
		_writeContent = new StringBuilder();
		_messageLog = new List<(string, DateTime, int, bool)>();
			
		_userName = userName;
		_sendPreprocessor = SendPreprocessor;
		_receivePreprocessor = ReceivePreprocessor;
			
		_client = new WebSocket4Net.WebSocket(uri, "", null, customHeaders);

		_client.Error += (sender, error) =>
		{
			Console.WriteLine($"Websocket error: {error.Exception.Message}");
		};

		_receiveProxy = new ReceiveProxy(_client);

		_client.Open();
	}
		
	public void ClientMain()
	{
		while (_client.State == WebSocket4Net.WebSocketState.Connecting)
			Thread.Sleep(15);
			
		DrawUI();


		int lastWidth = Console.WindowWidth;
		int lastHeight = Console.WindowHeight;


		while (_client.State == WebSocket4Net.WebSocketState.Open)
		{
			if (lastWidth != Console.WindowWidth || Console.WindowHeight != lastHeight)
			{
				lastWidth = Console.WindowWidth;
				lastHeight = Console.WindowHeight;

				DrawUI();
			}

			if (_receiveProxy.Available > 0)
			{
				var rawReceived = Encoding.UTF8.GetString(_receiveProxy.Receive());
				var receivedMessage =
					_receivePreprocessor != null?
						_receivePreprocessor(rawReceived) :
						rawReceived;
					

				_messageLog.Add((receivedMessage, DateTime.Now, 0, false));
				DrawUI();
			}
				
			if (Console.KeyAvailable)
			{
				ConsoleKeyInfo keyInfo = Console.ReadKey(true);
					
				if (keyInfo.Key != ConsoleKey.Enter)
				{
					if (keyInfo.Key == ConsoleKey.Backspace)
					{
						if (_writeContent.Length > 0)
						{
							_writeContent.Remove(_writeContent.Length - 1, 1);
							DrawUI();
						}
					}
					else
					{
						Console.Write(keyInfo.KeyChar);
						_writeContent.Append(keyInfo.KeyChar);
					}
				}
				else
				{
					if (String.Equals(_writeContent.ToString(), "/exit"))
					{
						_client.Close();
						continue;
					}
						
					var sendString =
						_sendPreprocessor != null ?
							_sendPreprocessor(_writeContent.ToString(), _userName) :
							_writeContent.ToString();

					if (sendString == null)
					{
						_messageLog.Add(("Command Error", DateTime.Now, 0, false));
						continue;
					}
						
					_client.Send(sendString);
						
					_messageLog.Add((sendString, DateTime.Now, 0, true));
						
					_writeContent.Clear();
					DrawUI();
				}

			}
		}
			
		Console.WriteLine("Connection dropped!");
		return;
	}
		
	void DrawUI()
	{
		Console.Clear();

		// Draws top bar.
		for (int i = 0; i < Console.WindowWidth * 2; i++)
			Console.Write('=');

		// Draws Side columns.
		for (int i = 0; i < Console.WindowHeight - 5; i++)
		{
			Console.SetCursorPosition(0, i + 2);
			Console.Write('|');
			Console.SetCursorPosition(Console.WindowWidth - 1, i + 2);
			Console.Write('|');
		}

		// Draws user text box
		for (int i = 0; i < Console.WindowWidth; i++)
			Console.Write('=');
		Console.Write("| Send: ");
		Console.SetCursorPosition(Console.WindowWidth - 1, Console.WindowHeight - 2);
		Console.Write('|');
		for (int i = 0; i < Console.WindowWidth; i++)
			Console.Write('=');

		// Draws received messages bottom to top with the latest one at the bottom.
		for (int i = 0; i < _messageLog.Count && i < Console.WindowHeight - 5; i++)
		{
			Console.SetCursorPosition(2, Console.WindowHeight - 4 - i);
			var message = _messageLog[_messageLog.Count - i - 1];
			// Checks if the message was sent by the user.
			if (message.sent)
				Console.Write($"> [{message.timestamp.ToLongTimeString()}] - \"{message.text}\"");
			// Or received from the server.
			else
				Console.Write($"< [{message.timestamp.ToLongTimeString()}] - \"{message.text}\" [RTT: {message.rtt} ms]");
		}

		// Sets console cursor position at the text box for the user.
		Console.SetCursorPosition(9, Console.WindowHeight - 2);
		if (_writeContent.Length > 0)
			Console.Write(_writeContent.ToString());
	}

	public class ReceiveProxy
	{
		public int Available
		{
			get
			{
				int totalLength = 0;
				foreach (string message in _receivedMessages)
					totalLength += message.Length;

				return totalLength;
			}
		}

		Queue<string> _receivedMessages = new Queue<string>();
		public ReceiveProxy(WebSocket4Net.WebSocket webSocket)
		{
			webSocket.MessageReceived += WebSocket_MessageReceived;
		}

		private void WebSocket_MessageReceived(object sender, WebSocket4Net.MessageReceivedEventArgs e)
		{
			_receivedMessages.Enqueue(e.Message);
		}

		public byte[] Receive()
		{
			return Encoding.UTF8.GetBytes(_receivedMessages.Dequeue());
		}
	}
}
