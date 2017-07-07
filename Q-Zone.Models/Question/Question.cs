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
    /// <summary>
    /// It is used for assigning question's difficulty.
    /// </summary>
    public enum Difficulty
    {
        None,
        Easy,
        Medium,
        Hard
    }

    public class Question
    {
        public const string QuestionTable = "Question";
        public const string QuestionTableID = "Question_ID";

        private const string AnswerOptionTable = "Answer_Option";
        private const string AnswerOptionTableID = "Option_ID";

        public int QuestionID { get; }

        public User Owner { get; private set; }

        public Topic Topic { get; }

        private string _questionString;
        public string QuestionString {
            get { return _questionString; }
            set {
                if (!IsLoggedInUserOwner()) return;
                if (string.IsNullOrWhiteSpace(value)) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuestionTable, "Question_String = :questionString", QuestionTableID + " = :questionID"),
                    new CommandParameter(":questionString", value),
                    new CommandParameter(":questionID", QuestionID));
                if (returnValue == 0) _questionString = value;
            }
        }

        private readonly int _correctAnswerID;
        private string _correctAnswer;
        public string CorrectAnswer {
            get { return _correctAnswer; }
            set {
                if (!IsLoggedInUserOwner()) return;
                if (string.IsNullOrWhiteSpace(value)) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    AnswerOptionTable, "Option_String = :optionString", AnswerOptionTableID + " = :optionID"),
                    new CommandParameter(":optionString", value),
                    new CommandParameter(":optionID", _correctAnswerID));
                if (returnValue == 0) _correctAnswer = value;
            }
        }

        private Difficulty _difficulty;
        public Difficulty DifficultyLevel {
            get { return _difficulty; }
            set {
                if (!IsLoggedInUserOwner()) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuestionTable, "Difficulty_Level = :difficulty", QuestionTableID + " = :questionID"),
                    new CommandParameter(":difficulty", ((int) value)),
                    new CommandParameter(":questionID", QuestionID));
                if (returnValue == 0) _difficulty = value;
            }
        }

        private readonly int[] _answerOptionIDList = {0, 0, 0, 0};
        private readonly string[] _answerOptionList = {"", "", "", ""};

        /// <summary>
        /// Edits one or more answer options
        /// </summary>
        /// <param name="option1">first answer option</param>
        /// <param name="option2">second answer option</param>
        /// <param name="option3">third answer option</param>
        /// <param name="option4">forth answer option</param>
        /// <returns>true if success, otherwise false</returns>
        public bool EditIncorrectOptions(string option1 = "", string option2 = "", string option3 = "",
            string option4 = "") {
            if (!IsLoggedInUserOwner()) return false;

            int usedParameters = 0;
            int successfulChanges = 0;

            string[] optionStrings = {option1, option2, option3, option4};

            for (int i = 0; i < optionStrings.Length; i++) {
                if (string.IsNullOrWhiteSpace(optionStrings[i])) continue;

                usedParameters++;
                bool isSuccessful = EditIncorrectOption(i, optionStrings[i]);
                if (isSuccessful) successfulChanges++;
            }
            return (usedParameters == successfulChanges);
        }

        /// <summary>
        /// Edits answer option by index (zero-based, from 0 to 3 inclusive)
        /// </summary>
        /// <param name="index">index of incorrect answer option (zero-based, from 0 to 3 inclusive)</param>
        /// <param name="answerOption">answer option string</param>
        /// <returns>true if success, otherwise false</returns>
        public bool EditIncorrectOption(int index, string answerOption) {
            if (!IsLoggedInUserOwner()) return false;
            if (index < 0 || index >= _answerOptionIDList.Length) return false;
            if (string.IsNullOrWhiteSpace(answerOption)) return false;

            int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                AnswerOptionTable, "Option_String = :optionString", AnswerOptionTableID + " = :optionID"),
                new CommandParameter(":optionString", answerOption),
                new CommandParameter(":optionID", _answerOptionIDList[index]));

            if (returnValue != 0) return false;

            _answerOptionList[index] = answerOption;
            return true;
        }

        /// <summary>
        /// Shows incorrect answer option by index (zero-based, from 0 to 3 inclusive)
        /// </summary>
        /// <param name="index">index of incorrect answer option (zero-based, from 0 to 3 inclusive)</param>
        /// <returns>incorrect answer option string</returns>
        public string ViewIncorrectAnswerOption(int index) {
            if (index < 0 || index >= _answerOptionIDList.Length) return null;

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Option_String", AnswerOptionTable, AnswerOptionTableID + " = :optionID"),
                new CommandParameter(":optionID", _answerOptionIDList[index]));

            return ((string) (dataTable.Rows[0][0]));
        }

        /// <summary>
        /// Shows all answer options alphabetically
        /// </summary>
        /// <returns>list of all answer options by alphabetical order</returns>
        public List<string> ViewAllAnswerOptions() {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Option_String", AnswerOptionTable, "Question_ID = :questionID", orderByClause: "Option_String"),
                new CommandParameter(":questionID", QuestionID));

            List<string> list = new List<string>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                list.Add((string) row[0]);
            }

            return list;
        }

        /// <summary>
        /// Gets the database optionID of the given option string
        /// </summary>
        /// <param name="optionString">option string</param>
        /// <returns>optionID of the given option string if found, otherwise -1</returns>
        public int GetOptionID(string optionString) {
            if (optionString == CorrectAnswer) return _correctAnswerID;
            for (int i = 0; i < _answerOptionList.Length; i++) {
                if (optionString == (_answerOptionList[i])) return (_answerOptionIDList[i]);
            }
            return -1;
        }

        /// <summary>
        /// Gets answer option by its database optionID
        /// </summary>
        /// <param name="optionID">optionID of answer option</param>
        /// <returns>answer option if found, otherwise null</returns>
        public string GetAnswerOptionByOptionID(int optionID) {
            if (optionID == _correctAnswerID) return CorrectAnswer;
            for (int i = 0; i < _answerOptionIDList.Length; i++) {
                if (optionID == (_answerOptionIDList[i])) return (_answerOptionList[i]);
            }
            return null;
        }

        /// <summary>
        /// Checks if logged-in user is the owner
        /// </summary>
        /// <returns>ture if logged-in user is the owner, otherwise false</returns>
        public bool IsLoggedInUserOwner() {
            return (this.Owner == UserAuthentication.LoggedInUser);
        }

        /// <summary>
        /// Deletes this question
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public bool Delete() {
            if (!IsLoggedInUserOwner()) return false;
            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                QuestionTable, QuestionTableID + " = :questionID"),
                new CommandParameter(":questionID", QuestionID));
            if (returnValue != 0) return false;

            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                Topic.TopicTable, "Total_Questions = :totalQuestions", Topic.TopicTableID + " = :topicID"),
                new CommandParameter("totalQuestions", (Topic.TotalQuestions - 1)),
                new CommandParameter(":topicID", Topic.TopicID));

            Owner = null;
            return true;
        }

        /// <summary>
        /// This is used for creating a new question
        /// </summary>
        /// <param name="topic">topic of the question to create</param>
        public Question(Topic topic) {
            if ((object) topic == null) {
                throw new ArgumentException("Topic cannot be null", nameof(topic));
            }

            if (!topic.IsLoggedInUserOwner()) {
                throw new InvalidCredentialException("User must be logged in before creating a question");
            }
            Owner = UserAuthentication.LoggedInUser;
            Topic = topic;

            int questionID;
            DataAccessLayer.InsertCommand_SpecificColumnAutoID(out questionID, QuestionTable,
                "Topic_ID", ":topicID", QuestionTableID,
                new CommandParameter(":topicID", Topic.TopicID));
            QuestionID = questionID;

            _questionString = "Question string";
            _difficulty = Difficulty.None;
            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(QuestionTable,
                "Question_String = :questionString, Difficulty_Level = :difficulty", QuestionTableID + " = :questionID"),
                new CommandParameter(":questionString", _questionString),
                new CommandParameter(":difficulty", ((int) _difficulty)),
                new CommandParameter(":questionID", QuestionID));

            int correctAnswerID;
            _correctAnswer = "Correct answer";
            DataAccessLayer.InsertCommand_AllColumnAutoID(out correctAnswerID, AnswerOptionTable,
                ":questionID, :optionString", AnswerOptionTableID,
                new CommandParameter(":questionID", QuestionID),
                new CommandParameter(":optionString", _correctAnswer));
            _correctAnswerID = correctAnswerID;

            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(QuestionTable,
                "Correct_Answer_ID = :optionID", QuestionTableID + " = :questionID"),
                new CommandParameter(":optionID", _correctAnswerID),
                new CommandParameter(":questionID", QuestionID));

            for (int i = 0; i < 4; i++) {
                string optionString = "Answer option " + (i + 1).ToString();

                _answerOptionList[i] = optionString;

                int answerOptionID;
                DataAccessLayer.InsertCommand_AllColumnAutoID(out answerOptionID, AnswerOptionTable,
                    ":questionID, :optionString", AnswerOptionTableID,
                    new CommandParameter(":questionID", QuestionID),
                    new CommandParameter(":optionString", optionString));
                _answerOptionIDList[i] = answerOptionID;
            }

            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                Topic.TopicTable, "Total_Questions = :totalQuestions", Topic.TopicTableID + " = :topicID"),
                new CommandParameter("totalQuestions", (Topic.TotalQuestions + 1)),
                new CommandParameter(":topicID", Topic.TopicID));
        }

        /// <summary>
        /// This is used for viewing or editing an already created question but not for creating a new question
        /// </summary>
        /// <param name="questionID">questionID of an already created question</param>
        /// <param name="cachedTopic">cached topic for optimization purpose by preventing multiple creations of same topic</param>
        public Question(int questionID, Topic cachedTopic = null) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Topic_ID, Question_String, Correct_Answer_ID, Difficulty_Level", QuestionTable,
                QuestionTableID + " = :questionID"), new CommandParameter(":questionID", questionID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("QuestionID does not exist");
            }
            QuestionID = questionID;

            int topicID = (int) (decimal) (dataTable.Rows[0]["Topic_ID"]);
            _questionString = (string) (dataTable.Rows[0]["Question_String"]);
            _correctAnswerID = (int) (decimal) (dataTable.Rows[0]["Correct_Answer_ID"]);
            _difficulty = (Difficulty) (int) (decimal) (dataTable.Rows[0]["Difficulty_Level"]);

            Topic = (cachedTopic?.TopicID == topicID) ? cachedTopic : new Topic(topicID);

            dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Topic.TopicTableID + ", Owner_ID", Topic.TopicTable, Topic.TopicTableID + " = :topicID"),
                new CommandParameter(":topicID", Topic.TopicID));
            int ownerID = (int) (decimal) (dataTable.Rows[0]["Owner_ID"]);
            Owner = (UserAuthentication.LoggedInUser?.UserID == ownerID)
                ? UserAuthentication.LoggedInUser
                : new User(ownerID);

            dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Option_String", AnswerOptionTable, AnswerOptionTableID + " = :optionID"),
                new CommandParameter(":optionID", _correctAnswerID));
            _correctAnswer = (string) (dataTable.Rows[0][0]);

            dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                AnswerOptionTableID + ", Option_String", AnswerOptionTable,
                "Question_ID = :questionID AND " + AnswerOptionTableID + " <> :optionID",
                orderByClause: AnswerOptionTableID),
                new CommandParameter(":questionID", QuestionID),
                new CommandParameter(":optionID", _correctAnswerID));

            for (int i = 0; i < dataTable.Rows.Count; i++) {
                _answerOptionIDList[i] = (int) (decimal) (dataTable.Rows[i][AnswerOptionTableID]);
                _answerOptionList[i] = (string) (dataTable.Rows[i]["Option_String"]);
            }
        }


        public override bool Equals(object obj) {
            return Equals(obj as Question);
        }

        public bool Equals(Question question) {
            if ((object) question == null) return false;
            return (this.QuestionID == question.QuestionID);
        }

        public override int GetHashCode() {
            return this.QuestionID.GetHashCode();
        }

        public static bool operator ==(Question question1, Question question2) {
            if (object.ReferenceEquals(question1, question2)) return true;
            if (((object) question1 == null) || ((object) question2 == null)) return false;
            return (question1.QuestionID == question2.QuestionID);
        }

        public static bool operator !=(Question question1, Question question2) {
            return !(question1 == question2);
        }
    }
}