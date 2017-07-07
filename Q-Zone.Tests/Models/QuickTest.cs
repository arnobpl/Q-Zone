using System;
using System.Collections.Generic;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.Question;
using Q_Zone.Models.Quiz;

namespace Q_Zone.Tests.Models
{
    [TestClass()]
    public class QuickTest
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
        public void QuickMethodTest() {
            UserAuthentication.Login(TestData.UserData.Username, TestData.UserData.Password);

            Topic topic = TopicBank.AddTopic("sdfdsf");
            Q_Zone.Models.Question.Question q1 = QuestionBank.AddQuestion(topic);
            Q_Zone.Models.Question.Question q2 = QuestionBank.AddQuestion(topic);
            Q_Zone.Models.Question.Question q3 = QuestionBank.AddQuestion(topic);
            Q_Zone.Models.Question.Question q4 = QuestionBank.AddQuestion(topic);
            Q_Zone.Models.Question.Question q5 = QuestionBank.AddQuestion(topic);
            Q_Zone.Models.Question.Question q6 = QuestionBank.AddQuestion(topic);

            Quiz quiz = QuizBank.AddQuiz(topic);
            quiz.QuizName = "dfgfdgdf";
            quiz.AddQuestion(q1);
            quiz.AddQuestion(q2);
            quiz.AddQuestion(q3);
            quiz.AddQuestion(q4);
            quiz.AddQuestion(q5);
            quiz.AddQuestion(q6);
            quiz.DateTime = DateTime.UtcNow.AddMilliseconds(5);
            quiz.IsPublic = true;

            AnswerSheet answerSheet = new AnswerSheet(quiz);
            bool testBool = answerSheet.GiveAnswer(q1, "Correct answer");
            answerSheet.GiveAnswer(q2, "Answer option 1");
            answerSheet.GiveAnswer(q3, "Correct answer");
            answerSheet.GiveAnswer(q4, "Correct answer");
            answerSheet.Submit();
            string testString = answerSheet.ShowGivenAnswer(q2);

            Result r1 = new Result(quiz);
            Result r2 = new Result(1, quiz);
            testBool = (r1 == r2);

            List<Quiz> quizList = RankList.ViewParticipatedQuizzes(maximumDuration: 5000);
            List<Result> rankList = RankList.ViewRankList(quiz);
            quizList = RankList.ViewStartedQuizzes(searchName: "d");
        }
    }
}