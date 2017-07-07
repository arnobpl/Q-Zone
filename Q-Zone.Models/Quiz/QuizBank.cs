using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;

namespace Q_Zone.Models.Quiz
{
    public static class QuizBank
    {
        /// <summary>
        /// Adds a new quiz to the question bank of 'LoggedInUser'
        /// </summary>
        /// <param name="topic">topic of the quiz to create</param>
        /// <returns>quiz if success, otherwise null</returns>
        public static Quiz AddQuiz(Topic topic) {
            try {
                return new Quiz(topic);
            }
            catch (Exception exception) {
                Debug.WriteLine("Error on creating a new quiz: " + exception.Message);
            }
            return null;
        }

        /// <summary>
        /// Shows a list of all quizzes or specific quizzes defined by parameters
        /// </summary>
        /// <param name="topic">quiz's topic or empty string for any topic</param>
        /// <param name="searchName">quiz's search name or empty string for any name</param>
        /// <param name="beginDateTime">quiz's begin date-time or null for unspecified begin date-time</param>
        /// <param name="endDateTime">quiz's begin date-time or null for unspecified end date-time</param>
        /// <param name="minimumDuration">quiz's minimum duration in second or 0 for any minimum duration</param>
        /// <param name="maximumDuration">quiz's maximum duration in second or -1 for any maximum duration</param>
        /// <returns>list of quizzes if success, otherwise null</returns>
        public static List<Quiz> ViewCreatedQuizzes(Topic topic = null, string searchName = "",
            DateTime? beginDateTime = null, DateTime? endDateTime = null, int minimumDuration = 0,
            int maximumDuration = -1) {
            if ((object) UserAuthentication.LoggedInUser == null) return null;

            string whereClause = "";
            List<CommandParameter> commandParameters = new List<CommandParameter>(6);

            if ((object) topic != null) {
                if (topic.Owner != UserAuthentication.LoggedInUser) return null;

                whereClause += " AND Topic_ID = :topicID";
                commandParameters.Add(new CommandParameter(":topicID", topic.TopicID));
            }
            else {
                whereClause += "Topic_ID IN (" + DataAccessLayer.SelectCommandString(
                    Topic.TopicTableID, Topic.TopicTable, "Owner_ID = :ownerID") + ")";
                commandParameters.Add(new CommandParameter(":ownerID", UserAuthentication.LoggedInUser.UserID));
            }

            if (searchName != "") {
                whereClause += " AND LOWER(Quiz_Name) LIKE LOWER(:quizName)";
                commandParameters.Add(new CommandParameter(":quizName", "%" + searchName + "%"));
            }

            if (beginDateTime != null) {
                whereClause += " AND Date_Time >= :beginDateTime";
                commandParameters.Add(new CommandParameter(":beginDateTime", beginDateTime));
            }

            if (endDateTime != null) {
                whereClause += " AND Date_Time <= :endDateTime";
                commandParameters.Add(new CommandParameter(":endDateTime", endDateTime));
            }

            if (minimumDuration != 0) {
                decimal minimumDurationDay = (minimumDuration * Quiz.SecondToDay);
                whereClause += " AND Duration_Day >= :minimumDurationDay";
                commandParameters.Add(new CommandParameter(":minimumDurationDay", minimumDurationDay));
            }

            if (maximumDuration != -1) {
                decimal maximumDurationDay = (maximumDuration * Quiz.SecondToDay);
                whereClause += " AND Duration_Day <= :maximumDurationDay";
                commandParameters.Add(new CommandParameter(":maximumDurationDay", maximumDurationDay));
            }

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Quiz.QuizTableID, Quiz.QuizTable, whereClause, orderByClause: "Date_Time DESC"),
                commandParameters.ToArray());

            List<Quiz> list = new List<Quiz>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add(new Quiz((int) (decimal) (row[0])));
            }

            return list;
        }
    }
}