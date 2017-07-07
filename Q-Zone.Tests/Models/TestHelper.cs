using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Web;
using System.Web.SessionState;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;
using Q_Zone.Tests.TestData;

namespace Q_Zone.Tests.Models
{
    public static class TestHelper
    {
        public static void TestInitialization(out int userID) {
            // initial cleanup
            DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                User.LoginTable, "Username = :username"),
                new CommandParameter(":username", UserData.Username));

            // prerequisite method call
            bool returnValue = UserAuthentication.SignUp(UserData.Username, UserData.Password, UserData.Email);
            Assert.IsTrue(returnValue, "\"SignUp\" method returns false");

            // assign userID
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                User.LoginTableID, User.LoginTable, "Username = :username"),
                new CommandParameter(":username", UserData.Username));
            Assert.AreEqual(1, dataTable.Rows.Count, "Prerequisite data not added to the database");
            userID = (int) (decimal) (dataTable.Rows[0][0]);

            HttpContext.Current = FakeHttpContext();
        }

        public static void TestCleanup() {
            HttpContext.Current = null;

            // remove test data from database
            DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                User.LoginTable, "Username = :username"),
                new CommandParameter(":username", UserData.Username));
        }

        public static HttpContext FakeHttpContext() {
            var httpRequest = new HttpRequest("", "http://tempuri.org", "");
            var stringWriter = new StringWriter();
            var httpResponse = new HttpResponse(stringWriter);
            var httpContext = new HttpContext(httpRequest, httpResponse);

            var sessionContainer = new HttpSessionStateContainer("id", new SessionStateItemCollection(),
                new HttpStaticObjectsCollection(), 10, true,
                HttpCookieMode.AutoDetect,
                SessionStateMode.InProc, false);

            SessionStateUtility.AddHttpSessionStateToContext(httpContext, sessionContainer);

            return httpContext;
        }

        public static List<string> GetQuestionStringListFromQuestionList(
            List<Q_Zone.Models.Question.Question> questionList) {
            List<string> questionStringList = new List<string>(questionList.Count);
            foreach (Q_Zone.Models.Question.Question question in questionList) {
                questionStringList.Add(question.QuestionString);
            }
            return questionStringList;
        }

        public static List<string> GetTopicNameListFromTopicList(List<Topic> topicList) {
            List<string> topicNameList = new List<string>(topicList.Count);
            foreach (Topic topic in topicList) {
                topicNameList.Add(topic.TopicName);
            }
            return topicNameList;
        }
    }
}