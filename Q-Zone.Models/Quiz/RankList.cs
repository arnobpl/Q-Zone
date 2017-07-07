using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;

namespace Q_Zone.Models.Quiz
{
    public static class RankList
    {
        /// <summary>
        /// Shows a list of all started created quizzes or specific quizzes defined by parameters
        /// </summary>
        /// <param name="topic">quiz's topic or empty string for any topic</param>
        /// <param name="searchName">quiz's search name or empty string for any name</param>
        /// <param name="beginDateTime">quiz's begin date-time or null for unspecified begin date-time</param>
        /// <param name="endDateTime">quiz's begin date-time or null for unspecified end date-time</param>
        /// <param name="minimumDuration">quiz's minimum duration in second or 0 for any minimum duration</param>
        /// <param name="maximumDuration">quiz's maximum duration in second or -1 for any maximum duration</param>
        /// <returns>list of started created quizzes if success, otherwise null</returns>
        public static List<Quiz> ViewStartedQuizzes(Topic topic = null, string searchName = "",
            DateTime? beginDateTime = null, DateTime? endDateTime = null, int minimumDuration = 0,
            int maximumDuration = -1) {
            List<Quiz> checkList = QuizBank.ViewCreatedQuizzes(topic, searchName, beginDateTime, endDateTime,
                minimumDuration, maximumDuration);
            if (checkList == null) return null;

            List<Quiz> returnList = new List<Quiz>(checkList.Count);
            foreach (Quiz quiz in checkList) {
                if (quiz.IsPublic && quiz.IsQuizStarted()) returnList.Add(quiz);
            }

            return returnList;
        }

        /// <summary>
        /// Shows a list of all participated quizzes or specific quizzes defined by parameters
        /// </summary>
        /// <param name="topic">quiz's topic or empty string for any topic</param>
        /// <param name="searchName">quiz's search name or empty string for any name</param>
        /// <param name="beginDateTime">quiz's begin date-time or null for unspecified begin date-time</param>
        /// <param name="endDateTime">quiz's begin date-time or null for unspecified end date-time</param>
        /// <param name="minimumDuration">quiz's minimum duration in second or 0 for any minimum duration</param>
        /// <param name="maximumDuration">quiz's maximum duration in second or -1 for any maximum duration</param>
        /// <returns>list of participated quizzes if success, otherwise null</returns>
        public static List<Quiz> ViewParticipatedQuizzes(Topic topic = null, string searchName = "",
            DateTime? beginDateTime = null, DateTime? endDateTime = null, int minimumDuration = 0,
            int maximumDuration = -1) {
            if ((object) UserAuthentication.LoggedInUser == null) return null;

            List<CommandParameter> commandParameters = new List<CommandParameter>(7);

            string whereClause = "Owner_ID = :ownerID";
            commandParameters.Add(new CommandParameter(":ownerID", UserAuthentication.LoggedInUser.UserID));

            bool anyParameterPassed = false;
            const string firstParameterWhereClause = " AND Quiz_ID IN (";

            if ((object) topic != null) {
                if (topic.Owner != UserAuthentication.LoggedInUser) return null;

                whereClause += firstParameterWhereClause;
                anyParameterPassed = true;

                whereClause += DataAccessLayer.SelectCommandString(
                    Quiz.QuizTableID, Quiz.QuizTable, "Topic_ID = :topicID");
                commandParameters.Add(new CommandParameter(":topicID", topic.TopicID));
            }

            if (searchName != "") {
                const string parameterWhereClause = "LOWER(Quiz_Name) LIKE LOWER(:quizName)";
                if (!anyParameterPassed) {
                    whereClause += firstParameterWhereClause + DataAccessLayer.SelectCommandString(
                        Quiz.QuizTableID, Quiz.QuizTable, parameterWhereClause);
                    anyParameterPassed = true;
                }
                else {
                    whereClause += " AND " + parameterWhereClause;
                }
                commandParameters.Add(new CommandParameter(":quizName", "%" + searchName + "%"));
            }

            if (beginDateTime != null) {
                const string parameterWhereClause = "Date_Time >= :beginDateTime";
                if (!anyParameterPassed) {
                    whereClause += firstParameterWhereClause + DataAccessLayer.SelectCommandString(
                        Quiz.QuizTableID, Quiz.QuizTable, parameterWhereClause);
                    anyParameterPassed = true;
                }
                else {
                    whereClause += " AND " + parameterWhereClause;
                }
                commandParameters.Add(new CommandParameter(":beginDateTime", beginDateTime));
            }

            if (endDateTime != null) {
                const string parameterWhereClause = "Date_Time <= :endDateTime";
                if (!anyParameterPassed) {
                    whereClause += firstParameterWhereClause + DataAccessLayer.SelectCommandString(
                        Quiz.QuizTableID, Quiz.QuizTable, parameterWhereClause);
                    anyParameterPassed = true;
                }
                else {
                    whereClause += " AND " + parameterWhereClause;
                }
                commandParameters.Add(new CommandParameter(":endDateTime", endDateTime));
            }

            if (minimumDuration != 0) {
                const string parameterWhereClause = "Duration_Day >= :minimumDurationDay";
                if (!anyParameterPassed) {
                    whereClause += firstParameterWhereClause + DataAccessLayer.SelectCommandString(
                        Quiz.QuizTableID, Quiz.QuizTable, parameterWhereClause);
                    anyParameterPassed = true;
                }
                else {
                    whereClause += " AND " + parameterWhereClause;
                }
                decimal minimumDurationDay = (minimumDuration * Quiz.SecondToDay);
                commandParameters.Add(new CommandParameter(":minimumDurationDay", minimumDurationDay));
            }

            if (maximumDuration != -1) {
                const string parameterWhereClause = "Duration_Day <= :maximumDurationDay";
                if (!anyParameterPassed) {
                    whereClause += firstParameterWhereClause + DataAccessLayer.SelectCommandString(
                        Quiz.QuizTableID, Quiz.QuizTable, parameterWhereClause);
                    anyParameterPassed = true;
                }
                else {
                    whereClause += " AND " + parameterWhereClause;
                }
                decimal maximumDurationDay = (maximumDuration * Quiz.SecondToDay);
                commandParameters.Add(new CommandParameter(":maximumDurationDay", maximumDurationDay));
            }

            if (anyParameterPassed) whereClause += ")";

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Quiz_ID", Result.ResultTable, whereClause),
                commandParameters.ToArray());

            List<Quiz> list = new List<Quiz>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add(new Quiz((int) (decimal) (row[0])));
            }

            return list.OrderByDescending(o => o.DateTime).ToList();
        }

        /// <summary>
        /// Shows rank list of specific quiz
        /// </summary>
        /// <param name="quiz">quiz to view its rank list</param>
        /// <returns>list of individual quiz results if success, otherwise null</returns>
        public static List<Result> ViewRankList(Quiz quiz) {
            if ((object) quiz == null) return null;
            if (!(quiz.IsPublic && quiz.IsQuizStarted())) return null;

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Result.ResultTableID, Result.ResultTable, "Quiz_ID = :quizID",
                orderByClause: "Obtained_Marks DESC"),
                new CommandParameter(":quizID", quiz.QuizID));

            List<Result> list = new List<Result>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add(new Result((int) (decimal) (row[0]), quiz));
            }

            return list;
        }
    }
}