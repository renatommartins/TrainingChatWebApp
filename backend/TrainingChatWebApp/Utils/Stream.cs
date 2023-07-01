namespace TrainingChatWebApp.Utils;

public class StreamUtils
{
	public static async Task<int> ReadLineFromStream (Stream bodyStream, byte[] buffer)
	{
		var isEof = false;
		var length = 0;
		var current = 0;
		var readByte = new byte[1];
		byte nextByte = 0;

		while (!isEof)
		{
			isEof = await bodyStream.ReadAtLeastAsync(readByte, 1, false) < 0;
			nextByte = readByte[0];
			if (nextByte != '\n' && nextByte != '\r') { break; }
		}
		
		while (!isEof)
		{
			nextByte = readByte[0];
			if (nextByte == '\n' || nextByte == '\r') { break; }
			buffer[current++] = nextByte;
			length++;
			isEof = await bodyStream.ReadAtLeastAsync(readByte, 1, false) < 0;
		}

		readByte[0] = 0;
		nextByte = 0;

		return length;
	}
}
