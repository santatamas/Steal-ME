namespace StealME.Server.Core.BLL
{
    using System;
    using System.Linq;

    using StealME.Server.Core.Security;
    using StealME.Server.Data.DAL;

    public static class UserLogic
    {
        public static bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            User user = GetUser(username);
            if (user != null)
            {
                if (PasswordHash.ValidatePassword(oldPassword, user.Password))
                {
                    user.Password = PasswordHash.CreateHash(newPassword);
                    DataHandler.GetContext().AcceptAllChanges();
                    return true;
                }
            }
            return false;
        }

        public static User GetUser(string userName)
        {
            var userQuery = DataHandler.GetContext().User.Where(u => u.Name.ToLower() == userName.ToLower());
            return userQuery.Any() ? userQuery.First() : null;
        }

        public static bool DeleteUser(string username)
        {
            User user = GetUser(username);
            if (user != null)
            {
                var context = DataHandler.GetContext();
                context.User.DeleteObject(user);
                context.SaveChanges();
                return true;
            }
            return false;
        }

        public static User CreateUser(string username, string password, string email, bool isApproved)
        {
            var context = DataHandler.GetContext();
            User user = new User
                {
                    Id = Guid.NewGuid(),
                    Name = username,
                    CreationDate = DateTime.Now,
                    LastModifiedDate = DateTime.Now,
                    LastLoginDate = DateTime.Now,
                    Password = PasswordHash.CreateHash(password),
                    Email = email,
                    IsActivated = true,
                    IsLockedOut = false
                };
            context.User.AddObject(user);
            context.SaveChanges();

            user = GetUser(user.Name);
            return user;
        }

        public static void UpdateUser(User user)
        {
            DataHandler.GetContext().AcceptAllChanges();
        }
    }
}