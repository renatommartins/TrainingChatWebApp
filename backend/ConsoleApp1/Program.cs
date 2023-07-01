// See https://aka.ms/new-console-template for more information

using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

var stringOne = "Lino impostor vagabundo";

Console.WriteLine(stringOne);

stringOne += " e traíra";

Console.WriteLine(stringOne);

var password = "mamalino";
var passwordBytesOriginal = Encoding.UTF8.GetBytes(password);

var salt = new byte[32];/*
{
	0x8d, 0x7c, 0x14, 0x7a, 0x8e, 0xd0, 0xf3, 0xd8,
	0x49, 0x42, 0x07, 0x25, 0x29, 0x47, 0xd0, 0x35,
	0x7b, 0x97, 0xc1, 0x50, 0xf7, 0xd9, 0xd6, 0xab,
	0x26, 0x2f, 0xb8, 0xc0, 0xd7, 0x5e, 0x38, 0x3d
};*/
RandomNumberGenerator.Fill(new Span<byte>(salt));

var passwordBytes = new byte[128];
Array.Copy(passwordBytesOriginal, 0, passwordBytes, 0, passwordBytesOriginal.Length);
//Array.Copy(salt, 0, passwordBytes, passwordBytesOriginal.Length, 32);

var hasher = new Argon2id(passwordBytes);
hasher.DegreeOfParallelism = 8;
hasher.MemorySize = 16 * 1024;
hasher.Iterations = 10;
hasher.Salt = salt;

var hashResult = await hasher.GetBytesAsync(16);

Console.WriteLine($"password: {password}");

Console.Write("password bytes: ");
Console.Write($"{passwordBytes[0]:x2}");
for (var i = 1; i < password.Length; i++)
{
	//Console.Write(':');
	Console.Write($"{passwordBytes[i]:x2}");
}
Console.Write('\n');

Console.Write("salt: ");
Console.Write($"{salt[0]:x2}");
for (var i = 1; i < 32; i++)
{
	//Console.Write(':');
	Console.Write($"{salt[i]:x2}");
}
Console.Write('\n');

Console.Write("hash: ");
Console.Write($"{hashResult[0]:x2}");
for (var i = 1; i < 16; i++)
{
	//Console.Write(':');
	Console.Write($"{hashResult[i]:x2}");
}
Console.Write('\n');

Console.WriteLine("aeHOOO");
