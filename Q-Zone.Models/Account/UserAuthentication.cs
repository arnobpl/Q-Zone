using System.Diagnostics;
using System.Security.Authentication;
using System.Web;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Utility;

namespace Q_Zone.Models.Account
{
    public static class UserAuthentication
    {
        private const string CurrentInstanceAccessString = "CurrentInstanceUser";

        public static User LoggedInUser {
            get { return HttpContext.Current.Session[CurrentInstanceAccessString] as User; }
            private set { HttpContext.Current.Session[CurrentInstanceAccessString] = value; }
        }

        /// <returns>true if success, otherwise false</returns>
        public static bool Login(string username, string password) {
            //TODO: Implement "Remember me" feature

            try {
                LoggedInUser = new User(username, password);
            }
            catch (InvalidCredentialException exception) {
                Debug.WriteLine("Error on login: " + exception.Message);
                return false;
            }

            Debug.WriteLine("Login successful for user: " + username);
            return true;
        }

        /// <returns>true if success, otherwise false</returns>
        public static bool SignUp(string username, string password, string email) {
            if (password.Length < User.MinimumPasswordLength || !ValidFormat.IsEmailFormat(email)) return false;

            int returnID = DataAccessLayer.InsertCommand_SpecificColumnAutoID(User.LoginTable,
                "Username, Password, Email", ":username, :password, :email", User.LoginTableID,
                new CommandParameter(":username", username),
                new CommandParameter(":password", Encryption.Encrypt(password, username)),
                new CommandParameter(":email", email));

            return (returnID == 0);
        }

        public static void Logout() {
            //LoggedInUser = null;
            HttpContext.Current.Session.Clear();
        }

        public static bool IsLoggedIn() {
            return ((object) LoggedInUser != null);
        }

        /// <summary>
        /// Deletes logged-in user's account
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public static bool DeleteAccount() {
            if (!IsLoggedIn()) return false;
            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                User.LoginTable, User.LoginTableID + " = :userID"),
                new CommandParameter(":userID", LoggedInUser.UserID));
            if (returnValue != 0) return false;
            Logout();
            return true;
        }
    }
}