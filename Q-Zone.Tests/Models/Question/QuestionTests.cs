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
    public class QuestionTests
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
        public void QuestionTest_Topic() {
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
            Q_Zone.Models.Question.Question question = null;
            try {
                question = new Q_Zone.Models.Question.Question(returnTopic);
            }
            catch (Exception exception) {
                Debug.WriteLine(exception.Message);
                Assert.Fail("\"Question(topic)\" constructor throws exception");
            }

            // initial check
            Assert.AreEqual(UserAuthentication.LoggedInUser, question.Owner, "Question's owner is not same as created");
            Assert.AreEqual(QuestionData.TopicName, question.Topic.TopicName, "Question's topic is not same as created");

            // extended check
            Assert.AreEqual("Question string", question.QuestionString,
                "Question's default question string is not same as intended");
            const string correctAnswer = "Correct answer";
            Assert.AreEqual(correctAnswer,
                question.CorrectAnswer, "Question's default correct answer is not same as intended");
            Assert.AreEqual(Difficulty.None,
                question.DifficultyLevel, "Question's default difficulty level is not same as intended");
            List<string> list = question.ViewAllAnswerOptions();
            const string errorMessage = "Question's answer option list is not same as intended";
            Assert.AreEqual(5, list.Count, errorMessage);
            for (int i = 0; i < 4; i++) {
                string answerOption = "Answer option " + (i + 1).ToString();
                Assert.AreEqual(answerOption, (list[i]), errorMessage);
            }
            Assert.AreEqual(correctAnswer, (list[4]), errorMessage);

            // security check
            UserAuthentication.Logout();
            question = null;
            try {
                question = new Q_Zone.Models.Question.Question(returnTopic);
            }
            catch (InvalidCredentialException exception) {
                Debug.WriteLine(exception.Message);
            }
            Assert.IsNull(question, "\"Question(topic)\" constructor does not throw exception even after logout");
        }

        [TestMethod()]
        public void QuestionTest_QuestionID() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);
            int questionID = question.QuestionID;

            // test method call
            question = new Q_Zone.Models.Question.Question(questionID);

            // initial check
            Assert.AreEqual(UserAuthentication.LoggedInUser, question.Owner, "Question's owner is not same as created");
            Assert.AreEqual(QuestionData.TopicName, question.Topic.TopicName, "Question's topic is not same as created");

            // extended check
            Assert.AreEqual("Question string", question.QuestionString,
                "Question's question string is not same as created");
            const string correctAnswer = "Correct answer";
            Assert.AreEqual(correctAnswer,
                question.CorrectAnswer, "Question's correct answer is not same as created");
            Assert.AreEqual(Difficulty.None,
                question.DifficultyLevel, "Question's difficulty level is not same as created");
            List<string> list = question.ViewAllAnswerOptions();
            const string errorMessage = "Question's answer option list is not same as created";
            Assert.AreEqual(5, list.Count, errorMessage);
            for (int i = 0; i < 4; i++) {
                string answerOption = "Answer option " + (i + 1).ToString();
                Assert.AreEqual(answerOption, (list[i]), errorMessage);
            }
            Assert.AreEqual(correctAnswer, (list[4]), errorMessage);
        }

        [TestMethod()]
        public void QuestionStringTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call
            Assert.AreEqual("Question string", question.QuestionString,
                "Question's question string is not same as created");
            const string newQuestionString = "This is a new question string";
            question.QuestionString = newQuestionString;

            // initial check
            Assert.AreEqual(newQuestionString, question.QuestionString,
                "Question's question string does not change even after login");

            // security check
            UserAuthentication.Logout();
            question.QuestionString = QuestionData.QuestionString;
            Assert.AreEqual(newQuestionString, question.QuestionString,
                "Question's question string changes even after logout");
        }

        [TestMethod()]
        public void CorrectAnswerTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call
            Assert.AreEqual("Correct answer", question.CorrectAnswer,
                "Question's correct answer is not same as created");
            const string newCorrectAnswer = "This is a new correct answer";
            question.CorrectAnswer = newCorrectAnswer;

            // initial check
            Assert.AreEqual(newCorrectAnswer, question.CorrectAnswer,
                "Question's correct answer does not change even after login");

            // security check
            UserAuthentication.Logout();
            question.CorrectAnswer = QuestionData.CorrectAnswer;
            Assert.AreEqual(newCorrectAnswer, question.CorrectAnswer,
                "Question's correct answer changes even after logout");
        }

        [TestMethod()]
        public void DifficultyTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call
            Assert.AreEqual(Difficulty.None, question.DifficultyLevel,
                "Question's difficulty level is not same as created");
            const Difficulty newDifficulty = Difficulty.Hard;
            question.DifficultyLevel = newDifficulty;

            // initial check
            Assert.AreEqual(newDifficulty, question.DifficultyLevel,
                "Question's difficulty level does not change even after login");

            // security check
            UserAuthentication.Logout();
            question.DifficultyLevel = Difficulty.Medium;
            Assert.AreEqual(newDifficulty, question.DifficultyLevel,
                "Question's difficulty level changes even after logout");
        }

        [TestMethod()]
        public void ViewAllAnswerOptionsTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call
            List<string> list = question.ViewAllAnswerOptions();

            // initial check
            Assert.AreEqual(5, list.Count, "\"ViewAllAnswerOptions\" method does not return all answer options");

            // extended check
            const string errorMessage = "\"ViewAllAnswerOptions\" method returns invalid answer option";
            for (int i = 0; i < 4; i++) {
                string answerOption = "Answer option " + (i + 1).ToString();
                Assert.AreEqual(answerOption, (list[i]), errorMessage);
            }
            Assert.AreEqual("Correct answer", list[4], errorMessage);
        }

        [TestMethod()]
        public void ViewIncorrectAnswerOptionTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call and extended check
            for (int i = 0; i < 4; i++) {
                string answerOption = "Answer option " + (i + 1).ToString();
                string returnAnswerOption = question.ViewIncorrectAnswerOption(i);
                Assert.AreEqual(answerOption, returnAnswerOption,
                    "\"ViewIncorrectAnswerOption\" method returns invalid answer option");
            }
        }

        [TestMethod()]
        public void EditIncorrectOptionsTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call
            returnValue = question.EditIncorrectOptions(QuestionData.AnswerOptions[0], QuestionData.AnswerOptions[1],
                QuestionData.AnswerOptions[2], QuestionData.AnswerOptions[3]);

            // initial check
            Assert.IsTrue(returnValue, "\"EditIncorrectOptions\" method returns false");

            // extended check
            List<string> list = question.ViewAllAnswerOptions();
            foreach (string answerOption in list) {
                returnValue = list.Contains(answerOption);
                Assert.IsTrue(returnValue, "Question's incorrect answer option not changed");
            }

            // security check
            UserAuthentication.Logout();
            returnValue = question.EditIncorrectOptions(option3: "changed answer option");
            Assert.IsFalse(returnValue, "Question's incorrect answer option changed even after logout");
            list = question.ViewAllAnswerOptions();
            foreach (string answerOption in list) {
                returnValue = list.Contains(answerOption);
                Assert.IsTrue(returnValue, "Question's incorrect answer option not changed");
            }
        }

        [TestMethod()]
        public void EditIncorrectOptionTest() {
            // prerequisite method call
            bool returnValue = UserAuthentication.Login(UserData.Username, UserData.Password);

            // prerequisite check
            Assert.IsTrue(returnValue, "\"Login\" method returns false");
            User user = UserAuthentication.LoggedInUser;
            Assert.IsNotNull(user, "\"LoggedInUser\" is null");

            // initialization
            Topic returnTopic = TopicBank.AddTopic(QuestionData.TopicName);
            Assert.IsNotNull(returnTopic, "\"AddTopic\" method returns null");
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);

            // test method call and initial check
            for (int i = 0; i < 4; i++) {
                returnValue = question.EditIncorrectOption(i, QuestionData.AnswerOptions[i]);
                Assert.IsTrue(returnValue, "\"EditIncorrectOption\" method returns false");
            }

            // extended check
            List<string> list = question.ViewAllAnswerOptions();
            foreach (string answerOption in list) {
                returnValue = list.Contains(answerOption);
                Assert.IsTrue(returnValue, "Question's incorrect answer option not changed");
            }

            // security check
            UserAuthentication.Logout();
            returnValue = question.EditIncorrectOption(2, "changed answer option");
            Assert.IsFalse(returnValue, "Question's incorrect answer option changed even after logout");
            list = question.ViewAllAnswerOptions();
            foreach (string answerOption in list) {
                returnValue = list.Contains(answerOption);
                Assert.IsTrue(returnValue, "Question's incorrect answer option not changed");
            }
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
            Q_Zone.Models.Question.Question question = new Q_Zone.Models.Question.Question(returnTopic);
            int questionID = question.QuestionID;

            // test method call
            returnValue = question.Delete();

            // initial check
            Assert.IsTrue(returnValue, "\"Delete\" method returns false");

            // initial database check
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Q_Zone.Models.Question.Question.QuestionTableID,
                Q_Zone.Models.Question.Question.QuestionTable,
                Q_Zone.Models.Question.Question.QuestionTableID + " = :questionID"),
                new CommandParameter(":questionID", questionID));
            Assert.AreEqual(0, dataTable.Rows.Count, "Data not deleted from the database");

            // extended check
            returnValue = question.Delete();
            Assert.IsFalse(returnValue, "\"Delete\" method returns true even for already deleted question");
        }
    }
}