using Microsoft.AspNetCore.Identity;
using TrainingChatApp.Models.Database;
using TrainingChatWebApp.Database.Models;
using TrainingChatWebApp.Services.Enums;

namespace TrainingChatWebApp.Services
{
    public interface IUserService
    {
        Session? Login(string username, byte[] passwordBuffer);
        ResultEnum Logout(string token);
        SignUpEnum SignUp(SignupModel signupModel);
        User? GetById(int key);

    }
}
