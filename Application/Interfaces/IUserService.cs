using Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Application.Interfaces
{
    public interface IUserService
    {
        /// <summary>
        /// check userexists
        /// </summary>
        /// <param name="username"></param>
        /// <param name="email"></param>
        /// <returns></returns>
        bool UserExists(string username, string email);
        
        /// <summary>
        /// create new user
        /// </summary>
        /// <param name="user"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        Task<UserModel> CreateUserAsync(UserModel user, string password);

        /// <summary>
        /// Authenticate user with email or username
        /// </summary>
        /// <param name="usernameOrEmail"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        UserModel Authenticate(string usernameOrEmail, string password);
        
        /// <summary>
        /// Jwt token
        /// </summary>
        /// <param name="user"></param>
        /// <returns></returns>
        string GenerateJwtToken(UserModel user);
    }
}
