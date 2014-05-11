namespace StealME.Server.Frontend.Web.App_Data
{
    using System;
    using System.Web.Security;

    using StealME.Server.Core.BLL;
    using StealME.Server.Core.Security;
    using StealME.Server.Data.DAL;

    public class StealMeMembershipProvider : MembershipProvider
    {
        public override string ApplicationName
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            return UserLogic.ChangePassword(username, oldPassword, newPassword);
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            MembershipUser newUser = GetMembershipUserFromCustomEntity(UserLogic.CreateUser(username, password, email, isApproved));

            status = newUser != null ? MembershipCreateStatus.Success : MembershipCreateStatus.ProviderError;

            return newUser;
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            return UserLogic.DeleteUser(username);
        }

        public override bool EnablePasswordReset
        {
            get { throw new NotImplementedException(); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { throw new NotImplementedException(); }
        }

        public override MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            throw new NotImplementedException();
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMembershipUserFromCustomEntity(UserLogic.GetUser(username));
        }

        public override MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            throw new NotImplementedException();
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { throw new NotImplementedException(); }
        }

        public override int MinRequiredPasswordLength
        {
            get
            {
                return 5;
            }
        }

        public override int PasswordAttemptWindow
        {
            get
            {
                return 10;
            }
        }

        public override MembershipPasswordFormat PasswordFormat
        {
            get { throw new NotImplementedException(); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { throw new NotImplementedException(); }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return false; }
        }

        public override bool RequiresUniqueEmail
        {
            get { return true; }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            throw new NotImplementedException();
        }

        public override void UpdateUser(MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            var user = UserLogic.GetUser(username);
            return user != null && PasswordHash.ValidatePassword(password, user.Password);
        }


        public static MembershipUser GetMembershipUserFromCustomEntity(User user)
        {
            if (user == null) return null;

            return new MembershipUser(
                "CustomMembershipProvider",
                user.Name,
                user.Id,
                user.Email,
                string.Empty,
                user.Comments,
                true,
                user.IsLockedOut,
                user.CreationDate,
                user.LastLoginDate ?? user.CreationDate,
                DateTime.Now,
                DateTime.Now,
                user.LastLockedOutDate ?? user.CreationDate);
        }
    }
}