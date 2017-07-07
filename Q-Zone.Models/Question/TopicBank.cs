using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;

namespace Q_Zone.Models.Question
{
    public static class TopicBank
    {
        /// <summary>
        /// Adds a new topic to the topic bank of 'LoggedInUser'
        /// </summary>
        /// <param name="topicName">topic name of the topic to create</param>
        /// <returns>topic if success, otherwise null</returns>
        public static Topic AddTopic(string topicName) {
            try {
                return new Topic(topicName);
            }
            catch (Exception exception) {
                Debug.WriteLine("Error on creating a new topic: " + exception.Message);
            }
            return null;
        }

        /// <summary>
        /// Shows a list of all topics or specific topics defined by parameter
        /// </summary>
        /// <param name="searchTopic">string to search by topic name or empty string for any topic name</param>
        /// <param name="minimumTotalQuestions">topic's minimum total number of questions or 0 for any minimum number of questions</param>
        /// <param name="maximumTotalQuestions">topic's maximum total number of questions or -1 for any maximum number of questions</param>
        /// <returns>list of topics if success, otherwise null</returns>
        public static List<Topic> ViewTopics(string searchTopic = "", int minimumTotalQuestions = 0,
            int maximumTotalQuestions = -1) {
            if ((object) UserAuthentication.LoggedInUser == null) return null;

            List<CommandParameter> commandParameters = new List<CommandParameter>(4);

            string whereClause = "Owner_ID = :ownerID";
            commandParameters.Add(new CommandParameter(":ownerID", UserAuthentication.LoggedInUser.UserID));

            if (searchTopic != "") {
                whereClause += " AND LOWER(Topic_Name) LIKE LOWER(:topicName)";
                commandParameters.Add(new CommandParameter(":topicName", "%" + searchTopic + "%"));
            }

            if (minimumTotalQuestions != 0) {
                whereClause += " AND Total_Questions >= :minimumTotalQuestions";
                commandParameters.Add(new CommandParameter(":minimumTotalQuestions", minimumTotalQuestions));
            }

            if (maximumTotalQuestions != -1) {
                whereClause += " AND Total_Questions <= :maximumTotalQuestions";
                commandParameters.Add(new CommandParameter(":maximumTotalQuestions", maximumTotalQuestions));
            }

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Topic.TopicTableID, Topic.TopicTable, whereClause, orderByClause: "Topic_Name"),
                commandParameters.ToArray());

            List<Topic> list = new List<Topic>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add(new Topic((int) (decimal) (row[0])));
            }

            return list;
        }
    }
}