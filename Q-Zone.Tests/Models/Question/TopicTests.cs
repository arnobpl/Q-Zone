using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Security.Authentication;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models.Question
{
    [TestClass()]
    public class TopicTests
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
        public void TopicTest_TopicName() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // test method call and initial check
            Topic returnTopic = null;
            try {
                returnTopic = new Topic(QuestionData.TopicName);
            }
            catch (Exception exception) {
                Debug.WriteLine(exception.Message);
                Assert.Fail("\"Topic(topicName)\" constructor throws exception");
            }

            // extended check
            Assert.AreEqual(UserAuthentication.LoggedInUser, returnTopic.Owner, "Topic's owner is not same as created");
            Assert.AreEqual(QuestionData.TopicName, returnTopic.TopicName, "Topic's topic name is not same as created");
            Assert.AreEqual(0, returnTopic.TotalQuestions,
                "The number of total questions of the newly created topic is not 0");

            // security check
            UserAuthentication.Logout();
            returnTopic = null;
            try {
                returnTopic = new Topic(QuestionData.TopicNames[0]);
            }
            catch (InvalidCredentialException exception) {
                Debug.WriteLine(exception.Message);
            }
            Assert.IsNull(returnTopic, "\"Topic(topicName)\" constructor does not throw exception even after logout");
        }

        [TestMethod()]
        public void TopicTest_TopicID() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            int topicID = returnTopic.TopicID;

            // test method call
            returnTopic = new Topic(topicID);

            // initial check
            Assert.AreEqual(UserAuthentication.LoggedInUser, returnTopic.Owner, "Topic's owner is not same as created");
            Assert.AreEqual(QuestionData.TopicName, returnTopic.TopicName, "Topic's topic name is not same as created");

            // extended check
            Assert.AreEqual(0, returnTopic.TotalQuestions,
                "The number of total questions of the newly created topic is not 0");
        }

        [TestMethod()]
        public void TotalQuestionsTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            for (int i = 0; i < 5; i++) {
                QuestionBank.AddQuestion(returnTopic);
            }

            // test method call
            int totalQuestions = returnTopic.TotalQuestions;

            // initial check
            Assert.AreEqual(5, totalQuestions, "The number of total questions of the topic is not correct");

            // extended check
            List<Q_Zone.Models.Question.Question> list = returnTopic.ViewQuestions();
            foreach (Q_Zone.Models.Question.Question questionObject in list) {
                questionObject.Delete();
            }
            totalQuestions = returnTopic.TotalQuestions;
            Assert.AreEqual(0, totalQuestions, "The number of total questions of the topic is not correct");
        }

        [TestMethod()]
        public void TopicNameTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Topic returnTopic0 = TopicBank.AddTopic(QuestionData.TopicNames[0]);

            // test method call
            returnTopic.TopicName = QuestionData.TopicNames[1];

            // initial check
            Assert.AreEqual(QuestionData.TopicNames[1], returnTopic.TopicName,
                "Topic's topic name does not change even after login");

            // initial database check
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Topic.TopicTableID, Topic.TopicTable, "Owner_ID = :ownerID AND Topic_Name = :topicName"),
                new CommandParameter(":ownerID", _userID), new CommandParameter(":topicName", QuestionData.TopicName));

            Assert.AreEqual(0, dataTable.Rows.Count, "Topic name not changed in the database");

            // extended check
            returnTopic0.TopicName = QuestionData.TopicNames[1];
            Assert.AreEqual(QuestionData.TopicNames[0], returnTopic0.TopicName,
                "Topic name changed even for duplicate name");
            returnTopic0.TopicName = QuestionData.TopicName;
            const string errorMessage = "Topic name not changed even for unique name";
            Assert.AreEqual(QuestionData.TopicName, returnTopic0.TopicName, errorMessage);
            returnTopic.TopicName = QuestionData.TopicNames[0];
            Assert.AreEqual(QuestionData.TopicNames[0], returnTopic.TopicName, errorMessage);

            // security check
            UserAuthentication.Logout();
            returnTopic.TopicName = QuestionData.TopicNames[1];
            Assert.AreEqual(QuestionData.TopicNames[0], returnTopic.TopicName, "Topic name changed even after logout");
        }

        [TestMethod()]
        public void ViewQuestionsTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = QuestionBank.AddQuestion(returnTopic);
            question.QuestionString = QuestionData.QuestionString;
            foreach (string questionString in QuestionData.QuestionStrings) {
                question = QuestionBank.AddQuestion(returnTopic);
                question.QuestionString = questionString;
            }

            // test method call
            List<Q_Zone.Models.Question.Question> list = returnTopic.ViewQuestions();

            // initial check
            Assert.AreEqual(5, list.Count, "\"ViewQuestions\" method does not return all questions");

            // initial database check
            List<string> questionStringList = TestHelper.GetQuestionStringListFromQuestionList(list);
            Assert.IsTrue(questionStringList.Contains(QuestionData.QuestionString), "Question not shown in the list");
            foreach (string questionString in QuestionData.QuestionStrings) {
                Assert.IsTrue(questionStringList.Contains(questionString), "Question not shown in the list");
            }

            // security check
            UserAuthentication.Logout();
            list = QuestionBank.ViewQuestions();
            Assert.IsNull(list, "Questions shown even after logout");
        }

        [TestMethod()]
        public void DeleteTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");

            // test method call
            returnValue = returnTopic.Delete();

            // initial check
            Assert.IsTrue(returnValue, "\"DeleteTopic\" method returns false");

            // initial database check
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Topic.TopicTableID, Topic.TopicTable, "Owner_ID = :ownerID AND Topic_Name = :topicName"),
                new CommandParameter(":ownerID", _userID), new CommandParameter(":topicName", QuestionData.TopicName));

            Assert.AreEqual(0, dataTable.Rows.Count, "Data not deleted from the database");

            // extended check
            returnValue = returnTopic.Delete();
            Assert.IsFalse(returnValue, "\"Delete\" method returns true even for already deleted topic");
        }
    }
}