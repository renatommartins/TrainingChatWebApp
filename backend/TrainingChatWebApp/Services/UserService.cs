using Dapper;
using Konscious.Security.Cryptography;
using MySql.Data.MySqlClient;
using System.Net;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services
{
    public class UserService : IUserService
    {
        public User? GetById(int key)
        {
            using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
            var user = (connection.Query<User>("""
					SELECT *
					FROM TrainingChatApp.Users
					WHERE Key = @key
					LIMIT 1
				""", new { key = key })).FirstOrDefault();

            return user;
        }

        public Session? Login(string username, byte[] passwordBuffer)
        {
            using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");
            var user = (connection.Query<User>("""
					SELECT *
					FROM TrainingChatApp.Users
					WHERE Username = @username
					LIMIT 1
				""", new { username = username })).FirstOrDefault();

            if (user is null)
            {
                return null;//Results.Unauthorized();
            }

            var hasher = new Argon2id(passwordBuffer);
            hasher.DegreeOfParallelism = 8;
            hasher.MemorySize = 16 * 1024;
            hasher.Iterations = 10;
            hasher.Salt = user.Salt;

            var hash = hasher.GetBytes(16);

            Array.Fill(passwordBuffer, (byte)0, 0, passwordBuffer.Length);

            for (var i = 0; i < 16; i++)
            {
                if (hash[i] == user.PasswordHash[i]) continue;
                return null;// Results.Unauthorized();
            }

            var session = new Session
            {
                UserKey = user.Key,
                SessionId = Guid.NewGuid(),
                ExpiresAt = DateTime.UtcNow.AddHours(16),
            };

            connection.Execute("""
					INSERT INTO TrainingChatApp.Sessions (UserKey, SessionId, ExpiresAt)
					VALUES (@UserKey, @SessionID, @ExpiresAt)
				""", new { UserKey = session.UserKey, SessionId = session.SessionId, ExpiresAt = session.ExpiresAt });

            return session; //200
        }

        public ResultEnum Logout(string token)
        {
            var (result, _, session) = AuthenticationService.AuthenticateSession(token);

            switch (result)
            {
                case ResultEnum.InvalidFormat:
                case ResultEnum.Unauthorized:
                    return result;
            }
            using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");

            var affectedRows = connection.Execute("""
				UPDATE TrainingChatApp.Sessions s
				SET s.IsLoggedOut = 1
				WHERE s.Key = @Key
			"""
            ,
                new { Key = session.Key });

            if (affectedRows != 1)
            {
                throw new Exception();
            }

            return result;
        }

        public SignUpEnum SignUp(SignupModel signupModel)
        {
            using var connection = new MySqlConnection("Server=localhost; User ID=root; Password=123456; Database=TrainingChatApp");

            var userCheck = (connection.Query<User>("""
					SELECT u.Username
					FROM TrainingChatApp.Users u
					WHERE Username = @Username
					LIMIT 1
				""", new { Username = signupModel.Username })).FirstOrDefault();

            if (userCheck is not null)
            {
                unsafe
                {
                    fixed (char* authPointer = signupModel.Password)
                        for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
                            *currentChar = '\0';
                }
                return SignUpEnum.UserAlreadyExists;//Results.Problem(detail: "User already exists", statusCode: (int)HttpStatusCode.Conflict);
            }

            var passwordBuffer = new byte[128];
            var salt = System.Security.Cryptography.RandomNumberGenerator.GetBytes(32);

            for (var i = 0; i < signupModel.Password.Length; i++)
            {
                passwordBuffer[i] = (byte)signupModel.Password[i];
            }

            unsafe
            {
                fixed (char* authPointer = signupModel.Password)
                    for (var currentChar = authPointer; *currentChar != '\0'; currentChar++)
                        *currentChar = '\0';
            }

            var hasher = new Argon2id(passwordBuffer);
            hasher.DegreeOfParallelism = 8;
            hasher.MemorySize = 16 * 1024;
            hasher.Iterations = 10;
            hasher.Salt = salt;

            var hash = hasher.GetBytes(16);

            Array.Fill(passwordBuffer, (byte)0x00);

            var rowsAffected = connection.Execute("""
					INSERT INTO TrainingChatApp.Users (Username, Name, PasswordHash, Salt)
					VALUES (@Username, @Name, @Password, @Salt)
				""", new { Username = signupModel.Username, Name = signupModel.Name, Password = hash, Salt = salt });

            if (rowsAffected != 1)
                throw new Exception();

            return SignUpEnum.UserCreated;
        }
    }
}
