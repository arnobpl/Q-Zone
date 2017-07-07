using System.Collections.Generic;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models.Question
{
    [TestClass()]
    public class TopicBankTests
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
        public void AddTopicTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // test method call
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);

            // initial check
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");

            // initial database check
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Topic_Name", Topic.TopicTable, "Owner_ID = :ownerID"),
                new CommandParameter(":ownerID", _userID));

            Assert.AreEqual(1, dataTable.Rows.Count, "Data not added to the database");
            Assert.AreEqual(QuestionData.TopicName, ((string) (dataTable.Rows[0][0])), "Topic name does not match");

            // extended check
            returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNull(returnTopic, "Topic added even for duplicate name");
            returnTopic = TopicBank.AddTopic(QuestionData.TopicNames[0]);
            Assert.IsNotNull(returnTopic, "Topic not added even for different name");

            // security check
            UserAuthentication.Logout();
            returnTopic = TopicBank.AddTopic(QuestionData.TopicNames[1]);
            Assert.IsNull(returnTopic, "Topic added even after logout");
        }

        [TestMethod()]
        public void ViewTopicsTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            foreach (string topicName in QuestionData.TopicNames) {
                TopicBank.AddTopic(topicName);
            }

            // test method call
            List<Topic> list = TopicBank.ViewTopics();

            // initial check
            Assert.AreEqual(5, list.Count, "\"ViewTopics\" method does not return all topics");

            // initial database check
            List<string> topicNameList = TestHelper.GetTopicNameListFromTopicList(list);
            const string errorMessage = "Topic not shown in the list";
            Assert.IsTrue(topicNameList.Contains(QuestionData.TopicName), errorMessage);
            foreach (string topicName in QuestionData.TopicNames) {
                Assert.IsTrue(topicNameList.Contains(topicName), errorMessage);
            }

            // extended check
            list = TopicBank.ViewTopics("DuMmY");
            Assert.AreEqual(2, list.Count, "\"ViewTopics\" method does not return all specified topics");
            for (int i = 0; i < 2; i++) {
                Assert.IsTrue(topicNameList.Contains(QuestionData.TopicNames[0]), errorMessage);
            }
            list = TopicBank.ViewTopics("never_found");
            Assert.AreEqual(0, list.Count, "\"ViewTopics\" method returns invalid topics");

            // security check
            UserAuthentication.Logout();
            list = TopicBank.ViewTopics();
            Assert.IsNull(list, "Topics shown even after logout");
        }
    }
}