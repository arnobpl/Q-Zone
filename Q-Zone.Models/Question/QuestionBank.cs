using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;

namespace Q_Zone.Models.Question
{
    public static class QuestionBank
    {
        /// <summary>
        /// Adds a new question to the question bank of 'LoggedInUser'
        /// </summary>
        /// <param name="topic">topic of the question to create</param>
        /// <returns>question if success, otherwise null</returns>
        public static Question AddQuestion(Topic topic) {
            try {
                return new Question(topic);
            }
            catch (Exception exception) {
                Debug.WriteLine("Error on creating a new question: " + exception.Message);
            }
            return null;
        }

        /// <summary>
        /// Shows a list of all questions or specific questions defined by parameters
        /// </summary>
        /// <param name="searchTopic">question's search topic or empty string for any topic</param>
        /// <param name="phrase">question's search phrase or empty string for any phrase</param>
        /// <param name="difficulty">question's difficulty or 'Difficulty.None' for any difficulty</param>
        /// <returns>list of questions if success, otherwise null</returns>
        public static List<Question> ViewQuestions(string searchTopic = "", string phrase = "",
            Difficulty difficulty = Difficulty.None) {
            if ((object) UserAuthentication.LoggedInUser == null) return null;

            string whereClause = "";
            List<CommandParameter> commandParameters = new List<CommandParameter>(4);

            if (searchTopic != "") {
                whereClause += "Topic_ID IN (" + DataAccessLayer.SelectCommandString(
                    Topic.TopicTableID, Topic.TopicTable,
                    "Owner_ID = :ownerID AND LOWER(Topic_Name) LIKE LOWER(:topicName)") + ")";
                commandParameters.Add(new CommandParameter(":ownerID", UserAuthentication.LoggedInUser.UserID));
                commandParameters.Add(new CommandParameter(":topicName", "%" + searchTopic + "%"));
            }
            else {
                whereClause += "Topic_ID IN (" + DataAccessLayer.SelectCommandString(
                    Topic.TopicTableID, Topic.TopicTable, "Owner_ID = :ownerID") + ")";
                commandParameters.Add(new CommandParameter(":ownerID", UserAuthentication.LoggedInUser.UserID));
            }

            if (phrase != "") {
                whereClause += " AND LOWER(Question_String) LIKE LOWER(:questionString)";
                commandParameters.Add(new CommandParameter(":questionString", "%" + phrase + "%"));
            }

            if (difficulty != Difficulty.None) {
                whereClause += " AND Difficulty_Level = :difficulty";
                commandParameters.Add(new CommandParameter(":difficulty", ((int) difficulty)));
            }

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Question.QuestionTableID, Question.QuestionTable, whereClause, orderByClause: "Question_String"),
                commandParameters.ToArray());

            List<Question> list = new List<Question>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add(new Question((int) (decimal) (row[0])));
            }

            return list;
        }
    }
}