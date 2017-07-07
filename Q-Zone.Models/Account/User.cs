using System.Data;
using System.Management.Instrumentation;
using System.Security.Authentication;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Utility;

// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeThisQualifier

namespace Q_Zone.Models.Account
{
    public class User
    {
        public const string LoginTable = "Login";
        public const string LoginTableID = "User_ID";
        public const int MinimumPasswordLength = 6;

        public int UserID { get; }

        public string Username { get; }

        private string _password;

        private string _email;
        public string Email {
            get { return _email; }
            set {
                if (this != UserAuthentication.LoggedInUser) return;
                if (!ValidFormat.IsEmailFormat(value)) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    LoginTable, "Email = :email", LoginTableID + " = :userID"),
                    new CommandParameter(":email", value),
                    new CommandParameter(":userID", UserID));
                if (returnValue == 0) _email = value;
            }
        }

        private string _name;
        public string Name {
            get { return _name; }
            set {
                if (this != UserAuthentication.LoggedInUser) return;
                if (string.IsNullOrWhiteSpace(value)) value = null;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    LoginTable, "Name = :name", LoginTableID + " = :userID"),
                    new CommandParameter(":name", value),
                    new CommandParameter(":userID", UserID));
                if (returnValue == 0) _name = value ?? "";
            }
        }

        public User(string username, string password) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                LoginTableID + ", Email, Name", LoginTable, "Username = :username AND Password = :password"),
                new CommandParameter(":username", username),
                new CommandParameter(":password", Encryption.Encrypt(password, username)));

            if (dataTable.Rows.Count != 1) {
                throw new InvalidCredentialException("Username or password is invalid");
            }

            UserID = (int) (decimal) (dataTable.Rows[0][LoginTableID]);
            Username = username;
            _password = password;
            _email = (string) (dataTable.Rows[0]["Email"]);
            _name = (dataTable.Rows[0]["Name"]) as string ?? "";
        }

        public User(int userID) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Username, Password, Email, Name", LoginTable, LoginTableID + " = :userID"),
                new CommandParameter(":userID", userID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("UserID does not exist");
            }

            UserID = userID;
            Username = (string) (dataTable.Rows[0]["Username"]);
            _password = (string) (dataTable.Rows[0]["Password"]);
            _email = (string) (dataTable.Rows[0]["Email"]);
            _name = (dataTable.Rows[0]["Name"]) as string ?? "";
        }

        /// <returns>true if success, otherwise false</returns>
        public bool ChangePassword(string currentPassword, string newPassword) {
            if (this != UserAuthentication.LoggedInUser) return false;
            if (_password != currentPassword) return false;
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < MinimumPasswordLength) return false;

            int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                LoginTable, "Password = :password", "Username = :username"),
                new CommandParameter(":password", Encryption.Encrypt(newPassword, Username)),
                new CommandParameter(":username", Username));
            if (returnValue != 0) return false;
            _password = newPassword;
            return true;
        }


        public override bool Equals(object obj) {
            return Equals(obj as User);
        }

        public bool Equals(User user) {
            if ((object) user == null) return false;
            return (this.UserID == user.UserID);
        }

        public override int GetHashCode() {
            return this.UserID.GetHashCode();
        }

        public static bool operator ==(User user1, User user2) {
            if (object.ReferenceEquals(user1, user2)) return true;
            if (((object) user1 == null) || ((object) user2 == null)) return false;
            return (user1.UserID == user2.UserID);
        }

        public static bool operator !=(User user1, User user2) {
            return !(user1 == user2);
        }
    }
}