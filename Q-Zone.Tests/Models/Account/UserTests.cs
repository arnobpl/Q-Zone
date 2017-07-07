using System;
using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models.Account
{
    [TestClass()]
    public class UserTests
    {
        // variable for test
        private int _userID;

        [TestInitialize()]
        public void TestInitialize() {
            TestHelper.TestInitialization(out _userID);
        }

        [TestCleanup()]
        public void TestCleanup() {
            TestHelper.TestCleanup();
        }

        [TestMethod()]
        public void UserTest_UsernamePassword() {
            // test method call
            User user = null;
            try {
                user = new User(UserData.Username, UserData.Password);
            }
            catch (Exception ex) {
                Debug.WriteLine(ex.Message);
                Assert.Fail("\"User(username, password)\" constructor throws exception");
            }

            // initial check
            Assert.IsNotNull(user, "\"User(username, password)\" constructor returns null.");

            // final check
            Assert.AreEqual(_userID, user.UserID, "UserID does not match");
            Assert.AreEqual(UserData.Username, user.Username, "Username does not match");
            Assert.AreEqual(UserData.Email, user.Email, "Email does not match");
            Assert.AreEqual("", user.Name, "Name does not match");

            // security check
            user = null;
            try {
                user = new User(UserData.Username, "wrong password");
            }
            catch (InvalidCredentialException ex) {
                Debug.WriteLine(ex.Message);
            }
            Assert.IsNull(user, "\"User(username, password)\" constructor " +
                                "does not throw exception even for invalid password");
        }

        [TestMethod()]
        public void UserTest_UserID() {
            // test method call
            User user = new User(_userID);

            // initial check
            Assert.IsNotNull(user, "\"User(userID)\" constructor returns null.");

            // final check
            Assert.AreEqual(_userID, user.UserID, "UserID does not match");
            Assert.AreEqual(UserData.Username, user.Username, "Username does not match");
            Assert.AreEqual(UserData.Email, user.Email, "Email does not match");
            Assert.AreEqual("", user.Name, "Name does not match");
        }

        [TestMethod()]
        public void ChangePasswordTest() {
            // dummy new password for test
            const string newPassword = "this_is_a_new_password";

            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initial check
            const string errorMessage = "Password not changed even for correct current password";
            returnValue = user.ChangePassword(UserData.Password, newPassword);
            Assert.IsTrue(returnValue, errorMessage);

            // extended check
            returnValue = user.ChangePassword(newPassword, UserData.Password);
            Assert.IsTrue(returnValue, errorMessage);
            returnValue = user.ChangePassword("this_is_a_wrong_password", newPassword);
            Assert.IsFalse(returnValue, "Password changed even for wrong current password");

            // security check
            UserAuthentication.Logout();
            returnValue = user.ChangePassword(UserData.Password, newPassword);
            Assert.IsFalse(returnValue, "Password changed even after logout");
        }

        [TestMethod()]
        public void EmailTest() {
            // dummy new email for test
            const string newEmail = "dummyNewEmail@dummyProvider.com";

            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initial check
            const string errorMessage = "Email not changed";
            user.Email = newEmail;
            Assert.AreEqual(newEmail, user.Email, errorMessage);

            // extended check
            user.Email = UserData.Email;
            Assert.AreEqual(UserData.Email, user.Email, errorMessage);

            // security check
            UserAuthentication.Logout();
            user.Email = newEmail;
            Assert.AreNotEqual(newEmail, user.Email, "Email changed even after logout");
        }

        [TestMethod()]
        public void NameTest() {
            // dummy new email for test
            const string newName = "Abc_new name";

            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initial check
            const string errorMessage = "Name not changed";
            user.Name = newName;
            Assert.AreEqual(newName, user.Name, errorMessage);

            // extended check
            user.Name = UserData.Name;
            Assert.AreEqual(UserData.Name, user.Name, errorMessage);

            // security check
            UserAuthentication.Logout();
            user.Name = newName;
            Assert.AreNotEqual(newName, user.Name, "Name changed even after logout");
        }
    }
}