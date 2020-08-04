using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace WebApplication1
{
    public interface IUserRepository
    {
        UserModel LogIn(string email, string password);
        UserModel Register(string email, string password);
    }


    public class UserRepository : IUserRepository
    {
        public UserModel LogIn(string email, string password)
        {
            var user = DatabaseAccessor.instance.User
                .FirstOrDefault(t => t.UserEmail.ToLower() == email.ToLower()
                                      && t.UserPassword == password);

            if (user == null)
            {
                return null;
            }

            return new UserModel { Id = user.UserId, Name = user.UserEmail };
        }

        public UserModel Register(string email, string password)
        {
            var user = DatabaseAccessor.instance.User.Add(new User { UserEmail = email, UserPassword = password });

            DatabaseAccessor.instance.SaveChanges();

            return new UserModel { Id = user.Entity.UserId, Name = user.Entity.UserEmail };
        }
    }
}