using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services.Interfaces;

public interface IUserService
{
    Session? Login(string username, byte[] passwordBuffer);
    Task<Session?> LoginAsync(string username, byte[] passwordBuffer);
    ResultEnum Logout(Guid token);
    Task<ResultEnum> LogoutAsync(Guid token);
    SignUpEnum SignUp(SignupModel signupModel);
    Task<SignUpEnum> SignUpAsync(SignupModel signupModel);
    User? GetById(int key);
    Task<User?> GetByKeyAsync(int key);

}
