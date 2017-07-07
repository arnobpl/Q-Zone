using System;
using System.Collections.Generic;
using System.Data;
using System.Management.Instrumentation;
using System.Security.Authentication;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;

// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeThisQualifier

namespace Q_Zone.Models.Question
{
    public class Topic
    {
        public const string TopicTable = "Topic";
        public const string TopicTableID = "Topic_ID";

        public int TopicID { get; }

        public User Owner { get; private set; }

        public int TotalQuestions {
            get {
                DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                    "Total_Questions", TopicTable, TopicTableID + " = :topicID"),
                    new CommandParameter(":topicID", TopicID));
                return ((int) (decimal) (dataTable.Rows[0][0]));
            }
        }

        private string _topicName;
        public string TopicName {
            get { return _topicName; }
            set {
                if (!IsLoggedInUserOwner()) return;
                if (string.IsNullOrWhiteSpace(value)) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    TopicTable, "Topic_Name = :topicName", TopicTableID + " = :topicID"),
                    new CommandParameter(":topicName", value),
                    new CommandParameter(":topicID", TopicID));
                if (returnValue == 0) _topicName = value;
            }
        }

        /// <summary>
        /// Shows a list of all questions of the topic or specific questions defined by parameters
        /// </summary>
        /// <param name="phrase">question's search phrase or empty string for any phrase</param>
        /// <param name="difficulty">question's difficulty or 'Difficulty.None' for any difficulty</param>
        /// <returns>list of questions if success, otherwise null</returns>
        public List<Question> ViewQuestions(string phrase = "", Difficulty difficulty = Difficulty.None) {
            List<CommandParameter> commandParameters = new List<CommandParameter>(3);

            string whereClause = TopicTableID + " = :topicID";
            commandParameters.Add(new CommandParameter(":topicID", TopicID));

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
                list.Add(new Question(((int) (decimal) (row[0])), this));
            }

            return list;
        }

        /// <summary>
        /// Checks if logged-in user is the owner
        /// </summary>
        /// <returns>ture if logged-in user is the owner, otherwise false</returns>
        public bool IsLoggedInUserOwner() {
            return (this.Owner == UserAuthentication.LoggedInUser);
        }

        /// <summary>
        /// Deletes this topic
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public bool Delete() {
            if (!IsLoggedInUserOwner()) return false;
            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                TopicTable, TopicTableID + " = :topicID"),
                new CommandParameter(":topicID", TopicID));
            if (returnValue != 0) return false;
            Owner = null;
            return true;
        }

        /// <summary>
        /// This is used for creating a new topic
        /// </summary>
        /// <param name="topicName">topic name of the topic to create</param>
        public Topic(string topicName) {
            if (string.IsNullOrWhiteSpace(topicName)) {
                throw new ArgumentException("Topic name cannot be null or whitespace", nameof(topicName));
            }

            if ((object) UserAuthentication.LoggedInUser == null) {
                throw new InvalidCredentialException("User must be logged in before creating a topic");
            }
            Owner = UserAuthentication.LoggedInUser;

            int topicID;
            int returnValue = DataAccessLayer.InsertCommand_SpecificColumnAutoID(out topicID, TopicTable,
                "Owner_ID, Topic_Name", ":ownerID, :topicName", TopicTableID,
                new CommandParameter(":ownerID", Owner.UserID),
                new CommandParameter(":topicName", topicName));

            if (returnValue != 0) {
                throw new DuplicateNameException("Topic name must be unique to the user");
            }
            TopicID = topicID;
            _topicName = topicName;
        }

        /// <summary>
        /// This is used for viewing or editing an already created topic but not for creating a new topic
        /// </summary>
        /// <param name="topicID">topicID of an already created topic</param>
        public Topic(int topicID) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Owner_ID, Topic_Name", TopicTable, TopicTableID + " = :topicID"),
                new CommandParameter(":topicID", topicID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("TopicID does not exist");
            }
            TopicID = topicID;

            int ownerID = (int) (decimal) (dataTable.Rows[0]["Owner_ID"]);
            Owner = (UserAuthentication.LoggedInUser?.UserID == ownerID)
                ? UserAuthentication.LoggedInUser
                : new User(ownerID);

            _topicName = (string) (dataTable.Rows[0]["Topic_Name"]);
        }


        public override bool Equals(object obj) {
            return Equals(obj as Topic);
        }

        public bool Equals(Topic topic) {
            if ((object) topic == null) return false;
            return (this.TopicID == topic.TopicID);
        }

        public override int GetHashCode() {
            return this.TopicID.GetHashCode();
        }

        public static bool operator ==(Topic topic1, Topic topic2) {
            if (object.ReferenceEquals(topic1, topic2)) return true;
            if (((object) topic1 == null) || ((object) topic2 == null)) return false;
            return (topic1.TopicID == topic2.TopicID);
        }

        public static bool operator !=(Topic topic1, Topic topic2) {
            return !(topic1 == topic2);
        }
    }
}