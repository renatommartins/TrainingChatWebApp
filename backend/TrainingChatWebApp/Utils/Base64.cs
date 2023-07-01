namespace TrainingChatWebApp.Utils;

public static class Base64
{
	private static readonly byte[] Base64Alphabet;
	private static readonly Dictionary<int, byte> ToBase64Dictionary;
	private static readonly Dictionary<byte, int> FromBase64Dictionary;
	
	static Base64()
	{
		Base64Alphabet = new byte[]
		{
			(byte)'A',(byte)'B',(byte)'C',(byte)'D',
			(byte)'E',(byte)'F',(byte)'G',(byte)'H',
			(byte)'I',(byte)'J',(byte)'K',(byte)'L',
			(byte)'M',(byte)'N',(byte)'O',(byte)'P',
		
			(byte)'Q',(byte)'R',(byte)'S',(byte)'T',
			(byte)'U',(byte)'V',(byte)'W',(byte)'X',
			(byte)'Y',(byte)'Z',(byte)'a',(byte)'b',
			(byte)'c',(byte)'d',(byte)'e',(byte)'f',
		
			(byte)'g',(byte)'h',(byte)'i',(byte)'j',
			(byte)'k',(byte)'l',(byte)'m',(byte)'n',
			(byte)'o',(byte)'p',(byte)'q',(byte)'r',
			(byte)'s',(byte)'t',(byte)'u',(byte)'v',
		
			(byte)'w',(byte)'x',(byte)'y',(byte)'z',
			(byte)'0',(byte)'1',(byte)'2',(byte)'3',
			(byte)'4',(byte)'5',(byte)'6',(byte)'7',
			(byte)'8',(byte)'9',(byte)'+',(byte)'/',
		};

		ToBase64Dictionary = new Dictionary<int, byte>();
		for (var i = 0; i < Base64Alphabet.Length; i++)
			ToBase64Dictionary.Add(i, Base64Alphabet[i]);

		FromBase64Dictionary = new Dictionary<byte, int>();
		for (var i = 0; i < Base64Alphabet.Length; i++)
			FromBase64Dictionary.Add(Base64Alphabet[i], i);
	}
	
	public static void FromBase64(ReadOnlySpan<byte> bytes, Span<byte> result)
	{
		var requiredLength = bytes.Length / 4 * 3;
		if (bytes[^1] == '=') requiredLength--;
		if (bytes[^2] == '=') requiredLength--;
		
		if (result.Length < requiredLength)
		{
			throw new Exception();
		}
		var offset = 0;

		var sextets = new byte[4];
		var decodedByte = (byte) 0;
		for (var i = 0; i < bytes.Length; i += 4)
		{
			do
			{
				sextets[0] = (byte)FromBase64Dictionary[bytes[i + 0]];
				sextets[1] = (byte)FromBase64Dictionary[bytes[i + 1]];

				decodedByte = (byte)(((sextets[0] & 0b00111111) << 2) | ((sextets[1] & 0b00110000) >> 4));
				result[offset++] = decodedByte;
				if (bytes[i + 2] == '=') { break; }

				sextets[2] = (byte)FromBase64Dictionary[bytes[i + 2]];
				decodedByte = (byte)(((sextets[1] & 0b00001111) << 4) | ((sextets[2] & 0b00111100) >> 2));
				result[offset++] = decodedByte;
				if (bytes[i + 3] == '=') { break; }

				sextets[3] = (byte)FromBase64Dictionary[bytes[i + 3]];
				decodedByte = (byte)(((sextets[2] & 0b00000011) << 6) | ((sextets[3] & 0b00111111) >> 0));
				result[offset++] = decodedByte;
			} while (false);
		}
		
		Array.Fill(sextets, (byte)0);
		decodedByte = 0;
	}
}
