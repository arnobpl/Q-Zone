using System.Data;
using System.Web;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Utility;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models.Account
{
    [TestClass()]
    public class UserAuthenticationTests
    {
        [TestInitialize()]
        public void TestInitialize() {
            // initial cleanup
            DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                User.LoginTable, "Username = :username"),
                new CommandParameter(":username", UserData.Username));

            HttpContext.Current = TestHelper.FakeHttpContext();
        }

        [TestCleanup()]
        public void TestCleanup() {
            TestHelper.TestCleanup();
        }

        [TestMethod()]
        public void SignUpTest() {
            // test method call
            bool returnValue = UserAuthentication.SignUp(UserData.Username, UserData.Password, UserData.Email);

            // initial check
            Assert.IsTrue(returnValue, "\"SignUp\" method returns false");

            // initial database check
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Password, Email", User.LoginTable, "Username = :username"),
                new CommandParameter(":username", UserData.Username));

            Assert.AreEqual(1, dataTable.Rows.Count, "Data not added to the database");
            Assert.AreEqual(Encryption.Encrypt(UserData.Password, UserData.Username), dataTable.Rows[0]["Password"],
                "Correct password not added to the database");
            Assert.AreEqual(UserData.Email, dataTable.Rows[0]["Email"], "Correct email not added to the database");

            // extended check
            returnValue = UserAuthentication.SignUp(UserData.Username, UserData.Password, "dummy@dummy.com");
            Assert.IsFalse(returnValue, "\"SignUp\" method returns true even for same username");
            returnValue = UserAuthentication.SignUp("I am dummy", UserData.Password, UserData.Email);
            Assert.IsFalse(returnValue, "\"SignUp\" method returns true even for same email");
        }

        [TestMethod()]
        public void LoginTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.SignUp(UserData.Username, UserData.Password, UserData.Email);
            Assert.IsTrue(returnValue, "\"SignUp\" method returns false");

            // prerequisite check
            Assert.IsTrue(returnValue, "\"SignUp\" method returns false");

            // test method call
            returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // initial check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");

            // extended check
            User user = UserAuthentication.LoggedInUser;

            Assert.IsNotNull(user, "\"LoggedInUser\" is null");
            Assert.AreEqual(UserData.Username, user.Username,
                "\"LoggedInUser\" is different from the actual logged-in user");

            // security check
            UserAuthentication.Logout();
            returnValue = UserAuthentication.Login(UserData.Username, "a wrong password");
            Assert.IsFalse(returnValue, "\"Login\" method returns true even for wrong password");
            returnValue = UserAuthentication.Login("wrong username", UserData.Password);
            Assert.IsFalse(returnValue, "\"Login\" method returns true even for wrong username");
        }
    }
}