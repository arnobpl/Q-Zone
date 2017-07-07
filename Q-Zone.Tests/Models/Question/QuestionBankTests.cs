using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.Question;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models.Question
{
    [TestClass()]
    public class QuestionBankTests
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
        public void AddQuestionTest() {
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
            Q_Zone.Models.Question.Question question = QuestionBank.AddQuestion(returnTopic);

            // initial check
            Assert.IsNotNull(question, "\"AddQuestion\" method returns null");

            // extended check
            Assert.AreEqual(UserAuthentication.LoggedInUser, question.Owner, "Question owner is not same as created");
            Assert.AreEqual(QuestionData.TopicName, question.Topic.TopicName, "Question's topic is not same as created");

            // security check
            UserAuthentication.Logout();
            question = QuestionBank.AddQuestion(returnTopic);
            Assert.IsNull(question, "\"AddQuestion\" method does not return null even after logout");
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
            Topic returnTopic0 = TopicBank.AddTopic(QuestionData.TopicNames[0]);

            Q_Zone.Models.Question.Question question = QuestionBank.AddQuestion(returnTopic);
            Assert.IsNotNull(question, "\"AddQuestion\" method returns null");
            question.QuestionString = QuestionData.QuestionString;
            question.DifficultyLevel = Difficulty.Hard;
            for (int i = 0; i < 3; i++) {
                question = QuestionBank.AddQuestion(returnTopic);
                question.QuestionString = QuestionData.QuestionStrings[i];
                question.DifficultyLevel = Difficulty.Easy;
            }
            question = QuestionBank.AddQuestion(returnTopic0);
            question.QuestionString = QuestionData.QuestionStrings[3];
            question.DifficultyLevel = Difficulty.Medium;

            // test method call
            List<Q_Zone.Models.Question.Question> list = QuestionBank.ViewQuestions();

            // initial check
            Assert.AreEqual(5, list.Count, "\"ViewQuestions\" method does not return all questions");

            // initial database check
            List<string> questionStringList = TestHelper.GetQuestionStringListFromQuestionList(list);
            Assert.IsTrue(questionStringList.Contains(QuestionData.QuestionString), "Question not shown in the list");
            foreach (string questionString in QuestionData.QuestionStrings) {
                Assert.IsTrue(questionStringList.Contains(questionString), "Question not shown in the list");
            }

            // extended check
            const string errorMessage = "\"ViewQuestions\" method does not return all specified questions";
            list = QuestionBank.ViewQuestions(QuestionData.TopicName);
            Assert.AreEqual(4, list.Count, errorMessage);
            questionStringList = TestHelper.GetQuestionStringListFromQuestionList(list);
            Assert.IsTrue(questionStringList.Contains(QuestionData.QuestionString), errorMessage);
            for (int i = 0; i < 3; i++) {
                Assert.IsTrue(questionStringList.Contains(QuestionData.QuestionStrings[i]), errorMessage);
            }
            list = QuestionBank.ViewQuestions(QuestionData.TopicName, "Dummy");
            Assert.AreEqual(3, list.Count, errorMessage);
            list = QuestionBank.ViewQuestions(QuestionData.TopicName, "Dummy", Difficulty.Easy);
            Assert.AreEqual(2, list.Count, errorMessage);
            list = QuestionBank.ViewQuestions(phrase: "Dummy");
            Assert.AreEqual(4, list.Count, errorMessage);
            list = QuestionBank.ViewQuestions(difficulty: Difficulty.Medium);
            Assert.AreEqual(1, list.Count, errorMessage);
            list = QuestionBank.ViewQuestions(QuestionData.TopicName, "", Difficulty.Easy);
            Assert.AreEqual(3, list.Count, errorMessage);
            list = QuestionBank.ViewQuestions("", "dummY", Difficulty.Hard);
            Assert.AreEqual(1, list.Count, errorMessage);

            // security check
            UserAuthentication.Logout();
            list = QuestionBank.ViewQuestions();
            Assert.IsNull(list, "Questions shown even after logout");
        }
    }
}