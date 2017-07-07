using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Management.Instrumentation;
using System.Security.Authentication;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;
using Q_Zone.Models.Question;

// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeThisQualifier
// ReSharper disable RedundantCast

namespace Q_Zone.Models.Quiz
{
    public class Quiz
    {
        public const string QuizTable = "Quiz";
        public const string QuizTableID = "Quiz_ID";

        private const string QuizQuestionTable = "Quiz_Question";
        private const string QuizQuestionTableID = "Quiz_Question_ID";

        public const int EachQuestionMarks = 5;
        public const int EachQuestionMinusMarks = 2;

        public const decimal SecondToDay = (1M / (24 * 60 * 60));

        public int QuizID { get; }

        public User Owner { get; private set; }

        public Topic Topic { get; }

        public int TotalParticipants {
            get {
                DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                    "Total_Participants", QuizTable, QuizTableID + " = :quizID"),
                    new CommandParameter(":quizID", QuizID));
                return ((int) (decimal) (dataTable.Rows[0][0]));
            }
        }

        private readonly List<int> _quizQuestionIDList;
        private readonly List<Question.Question> _questionList;

        public int TotalQuestions => _questionList.Count;

        public int TotalMarks => (TotalQuestions * EachQuestionMarks);

        public int AverageMarks {
            get {
                DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                    "Average_Marks", QuizTable, QuizTableID + " = :quizID"),
                    new CommandParameter(":quizID", QuizID));
                return ((int) (decimal) (dataTable.Rows[0][0]));
            }
        }

        private string _quizName;
        public string QuizName {
            get { return _quizName; }
            set {
                if (!IsQuizEditableForLoggedInUser()) return;
                if (string.IsNullOrWhiteSpace(value)) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuizTable, "Quiz_Name = :quizName", QuizTableID + " = :quizID"),
                    new CommandParameter(":quizName", value),
                    new CommandParameter(":quizID", QuizID));
                if (returnValue == 0) _quizName = value;
            }
        }

        private DateTime _endDateTimeUtc;
        private DateTime _dateTimeUtc;
        private DateTime _dateTime;
        public DateTime DateTime {
            get { return _dateTime; }
            set {
                if (!IsQuizEditableForLoggedInUser()) return;

                DateTime valueUtc = value.ToUniversalTime();
                if (valueUtc <= DateTime.UtcNow) return;

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuizTable, "Date_Time = :dateTime", QuizTableID + " = :quizID"),
                    new CommandParameter(":dateTime", valueUtc),
                    new CommandParameter(":quizID", QuizID));
                if (returnValue == 0) {
                    _dateTime = value;
                    _dateTimeUtc = valueUtc;
                    _endDateTimeUtc = _dateTimeUtc.AddSeconds(Duration);
                }
            }
        }

        private decimal _durationDay;
        private int _duration;
        /// <summary>
        /// Duration in second
        /// </summary>
        public int Duration {
            get { return _duration; }
            set {
                if (!IsQuizEditableForLoggedInUser()) return;
                if (value <= 0) return;

                decimal durationDay = (value * SecondToDay);

                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuizTable, "Duration_Day = :durationDay", QuizTableID + " = :quizID"),
                    new CommandParameter(":durationDay", durationDay),
                    new CommandParameter(":quizID", QuizID));
                if (returnValue == 0) {
                    _duration = value;
                    _durationDay = durationDay;
                    _endDateTimeUtc = _dateTimeUtc.AddSeconds(_duration);
                }
            }
        }

        private bool _isPublic;
        public bool IsPublic {
            get { return _isPublic; }
            set {
                if (!IsQuizEditableForLoggedInUser()) return;
                if (value == _isPublic) return;

                int isPublic = (value ? 1 : 0);
                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(
                    QuizTable, "Is_Public = :isPublic", QuizTableID + " = :quizID"),
                    new CommandParameter(":isPublic", isPublic),
                    new CommandParameter(":quizID", QuizID));
                if (returnValue == 0) _isPublic = value;
            }
        }

        private static readonly Random Random = new Random();

        /// <summary>
        /// Gets random questions in the given criteria (excluding already added questions)
        /// </summary>
        /// <param name="numberOfQuestions">number of random questions</param>
        /// <param name="phrase">random questions' search phrase or empty string for any phrase</param>
        /// <param name="difficulty">random questions' difficulty or 'Difficulty.None' for any difficulty</param>
        /// <returns>list of random questions if success, otherwise null</returns>
        public List<Question.Question> GetRandomQuestions(int numberOfQuestions, string phrase = "",
            Difficulty difficulty = Difficulty.None) {
            if (numberOfQuestions <= 0) return null;
            List<Question.Question> questions = Topic.ViewQuestions(phrase, difficulty);
            foreach (Question.Question question in _questionList) {
                questions.Remove(question);
            }
            if (questions.Count <= numberOfQuestions) return questions;

            int questionsToRemove = (questions.Count - numberOfQuestions);
            for (int i = 0; i < questionsToRemove; i++) {
                int questionIndexToRemove = Random.Next(questions.Count);
                questions.RemoveAt(questionIndexToRemove);
            }

            return questions;
        }

        private bool AddQuestionCore(Question.Question question) {
            if (this.Topic != question.Topic) return false;
            if (_questionList.IndexOf(question) != -1) return false;

            int quizQuestionID;
            int returnValue = DataAccessLayer.InsertCommand_AllColumnAutoID(out quizQuestionID, QuizQuestionTable,
                ":quizID, :questionID, :questionOrder", QuizQuestionTableID,
                new CommandParameter(":quizID", QuizID),
                new CommandParameter("quiestionID", question.QuestionID),
                new CommandParameter(":questionOrder", _quizQuestionIDList.Count));
            if (returnValue != 0) return false;
            _quizQuestionIDList.Add(quizQuestionID);
            _questionList.Add(question);
            return true;
        }

        /// <summary>
        /// Adds the given question to the quiz
        /// </summary>
        /// <param name="question">question to add</param>
        /// <returns>true if success, otherwise false</returns>
        public bool AddQuestion(Question.Question question) {
            if (!IsQuizEditableForLoggedInUser()) return false;
            if ((object) question == null) return false;
            return AddQuestionCore(question);
        }

        /// <summary>
        /// Adds the given list of questions to the quiz
        /// </summary>
        /// <param name="questions">list of questions to add</param>
        /// <returns>true if success, otherwise false</returns>
        public bool AddQuestions(List<Question.Question> questions) {
            if (!IsQuizEditableForLoggedInUser()) return false;
            if (questions == null) return false;

            int numberOfQuestions = questions.Count;
            int numberOfSuccessfullyAdded = 0;
            foreach (Question.Question question in questions) {
                bool isSuccessful = AddQuestionCore(question);
                if (isSuccessful) numberOfSuccessfullyAdded++;
            }
            return (numberOfQuestions == numberOfSuccessfullyAdded);
        }

        /// <summary>
        /// Changes all question orders in the given range 
        /// </summary>
        /// <param name="startIndex">start index of the question list</param>
        /// <param name="count">number of question orders to be changed</param>
        /// <param name="firstOrder">order to set in the start index</param>
        /// <returns>true if success, otherwise false</returns>
        private bool ChangeQuestionOrders(int startIndex, int count, int firstOrder) {
            int successfullyChanged = 0;
            int index = startIndex;
            int questionOrder = firstOrder;
            for (int i = 0; i < count; i++) {
                int returnValue = DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(QuizQuestionTable,
                    "Question_Order = :questionOrder", QuizQuestionTableID + " = :quizQuestionID"),
                    new CommandParameter(":questionOrder", questionOrder),
                    new CommandParameter(":quizQuestionID", _quizQuestionIDList[index]));
                if (returnValue == 0) successfullyChanged++;
                index++;
                questionOrder++;
            }
            return (count == successfullyChanged);
        }

        private bool DeleteQuestionCore(Question.Question question) {
            int findIndex = _questionList.IndexOf(question);
            if (findIndex == -1) return false;

            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(QuizQuestionTable,
                QuizQuestionTableID + " = :quizQuestionID"),
                new CommandParameter(":quizQuestionID", _quizQuestionIDList[findIndex]));
            if (returnValue != 0) return false;
            _quizQuestionIDList.RemoveAt(findIndex);
            _questionList.RemoveAt(findIndex);
            return true;
        }

        /// <summary>
        /// Deletes the given question to the quiz
        /// </summary>
        /// <param name="question">question to delete</param>
        /// <returns>true if success, otherwise false</returns>
        public bool DeleteQuestion(Question.Question question) {
            if (!IsQuizEditableForLoggedInUser()) return false;
            if ((object) question == null) return false;
            return DeleteQuestionCore(question);
        }

        /// <summary>
        /// Deletes the given list of questions to the quiz
        /// </summary>
        /// <param name="questions">list of questions to delete</param>
        /// <returns>true if success, otherwise false</returns>
        public bool DeleteQuestions(List<Question.Question> questions) {
            if (!IsQuizEditableForLoggedInUser()) return false;
            if (questions == null) return false;

            int numberOfQuestions = questions.Count;
            int numberOfSuccessfullyDeleted = 0;
            foreach (Question.Question question in questions) {
                bool isSuccessful = DeleteQuestionCore(question);
                if (isSuccessful) numberOfSuccessfullyDeleted++;
            }
            return (numberOfQuestions == numberOfSuccessfullyDeleted);
        }

        /// <summary>
        /// Deletes all questions from the quiz
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public bool DeleteAllQuestions() {
            if (!IsQuizEditableForLoggedInUser()) return false;
            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                QuizQuestionTable, "Quiz_ID = :quizID"),
                new CommandParameter(":quizID", QuizID));
            if (returnValue != 0) return false;
            _quizQuestionIDList.Clear();
            _questionList.Clear();
            return true;
        }

        /// <summary>
        /// Reorders questions by moving the question from the current order to the new order
        /// </summary>
        /// <param name="currentOrder">zero-based current order from which the question will be moved</param>
        /// <param name="newOrder">zero-based new order to which the question will be moved</param>
        /// <returns>true if success, otherwise false</returns>
        public bool ReorderQuestion(int currentOrder, int newOrder) {
            if (!IsQuizEditableForLoggedInUser()) return false;
            if (currentOrder < 0 || currentOrder >= _quizQuestionIDList.Count) return false;
            if (newOrder < 0 || newOrder >= _quizQuestionIDList.Count) return false;

            if (currentOrder == newOrder) return true;

            bool isSuccessful;
            if (currentOrder < newOrder) {
                isSuccessful = ChangeQuestionOrders(currentOrder + 1, newOrder - currentOrder, currentOrder);
            }
            else {
                isSuccessful = ChangeQuestionOrders(newOrder, currentOrder - newOrder, newOrder + 1);
            }
            if (!isSuccessful) return false;

            int quizQuestionID = _quizQuestionIDList[currentOrder];
            Question.Question question = _questionList[currentOrder];
            _quizQuestionIDList.RemoveAt(currentOrder);
            _questionList.RemoveAt(currentOrder);
            _quizQuestionIDList.Insert(newOrder, quizQuestionID);
            _questionList.Insert(newOrder, question);

            return true;
        }

        /// <summary>
        /// Gets the list of questions added in the quiz
        /// </summary>
        /// <returns>list of questions added in the quiz</returns>
        public List<Question.Question> ViewQuestions() {
            return _questionList.ToList();
        }

        /// <summary>
        /// Checks if quiz started (even not public)
        /// </summary>
        /// <returns>true if quiz started (even not public), otherwise false</returns>
        public bool IsQuizStarted() {
            return (_dateTimeUtc <= DateTime.UtcNow);
        }

        /// <summary>
        /// Checks if quiz finished (even not public)
        /// </summary>
        /// <returns>true if quiz finished (even not public), otherwise false</returns>
        public bool IsQuizFinished() {
            return (_endDateTimeUtc <= DateTime.UtcNow);
        }

        /// <summary>
        /// Checks if quiz running currently (even not public)
        /// </summary>
        /// <returns>true if quiz running currently (even not public), otherwise false</returns>
        public bool IsQuizRunning() {
            return (IsQuizStarted() && !IsQuizFinished());
        }

        /// <summary>
        /// Gets spent time in second (even not public)
        /// </summary>
        /// <returns>spent time in second (even not public)</returns>
        public int GetSpentTime() {
            if (!IsQuizStarted()) return 0;
            if (IsQuizFinished()) return Duration;
            return ((int) (DateTime.UtcNow - _dateTimeUtc).TotalSeconds);
        }

        /// <summary>
        /// Gets remaining time in second (even not public)
        /// </summary>
        /// <returns>remaining time in second (even not public)</returns>
        public int GetRemainingTime() {
            if (!IsQuizStarted()) return Duration;
            if (IsQuizFinished()) return 0;
            return ((int) ((_endDateTimeUtc - DateTime.UtcNow).TotalSeconds));
        }

        /// <summary>
        /// Checks if this quiz is editable for logged-in user
        /// </summary>
        /// <returns>true if editable, otherwise false</returns>
        private bool IsQuizEditableForLoggedInUser() {
            return (IsLoggedInUserOwner() && !(IsPublic && IsQuizRunning()));
        }

        /// <summary>
        /// Checks if logged-in user is the owner
        /// </summary>
        /// <returns>ture if logged-in user is the owner, otherwise false</returns>
        public bool IsLoggedInUserOwner() {
            return (this.Owner == UserAuthentication.LoggedInUser);
        }

        /// <summary>
        /// Deletes this quiz
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public bool Delete() {
            if (!IsQuizEditableForLoggedInUser()) return false;
            int returnValue = DataAccessLayer.DeleteCommand(DataAccessLayer.DeleteCommandString(
                QuizTable, QuizTableID + " = :quizID"),
                new CommandParameter(":quizID", QuizID));
            if (returnValue != 0) return false;
            Owner = null;
            _isPublic = false;
            return true;
        }

        /// <summary>
        /// This is used for creating a new quiz
        /// </summary>
        /// <param name="topic">topic of the quiz to create</param>
        public Quiz(Topic topic) {
            if ((object) topic == null) {
                throw new ArgumentException("Topic cannot be null", nameof(topic));
            }

            if (!topic.IsLoggedInUserOwner()) {
                throw new InvalidCredentialException("User must be logged in before creating a quiz");
            }
            Owner = UserAuthentication.LoggedInUser;
            Topic = topic;

            int quizID;
            DataAccessLayer.InsertCommand_SpecificColumnAutoID(out quizID, QuizTable, "Topic_ID", ":topicID",
                QuizTableID, new CommandParameter(":topicID", Topic.TopicID));
            QuizID = quizID;

            _quizName = "Quiz Name";
            _dateTime = DateTime.Now.AddDays(7);
            _dateTimeUtc = _dateTime.ToUniversalTime();
            _duration = (60 * 60);
            _durationDay = (_duration * SecondToDay);
            _endDateTimeUtc = _dateTimeUtc.AddSeconds(_duration);
            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(QuizTable,
                "Quiz_Name = :quizName, Date_Time = :dateTime, Duration_Day = :durationDay", QuizTableID + " = :quizID"),
                new CommandParameter(":quizName", _quizName),
                new CommandParameter(":dateTime", _dateTimeUtc),
                new CommandParameter(":durationDay", _durationDay),
                new CommandParameter(":quizID", QuizID));

            _isPublic = false;

            _quizQuestionIDList = new List<int>();
            _questionList = new List<Question.Question>();
        }

        /// <summary>
        /// This is used for viewing or editing an already created quiz but not for creating a new quiz
        /// </summary>
        /// <param name="quizID">quizID of an already created quiz</param>
        /// <param name="cachedTopic">cached topic for optimization purpose by preventing multiple creations of same topic</param>
        public Quiz(int quizID, Topic cachedTopic = null) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Topic_ID, Quiz_Name, Date_Time, Duration_Day, Is_Public", QuizTable,
                QuizTableID + " = :quizID"), new CommandParameter(":quizID", quizID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("QuizID does not exist");
            }
            QuizID = quizID;

            int topicID = (int) (decimal) (dataTable.Rows[0]["Topic_ID"]);
            _quizName = (string) (dataTable.Rows[0]["Quiz_Name"]);
            _dateTimeUtc = (DateTime) (dataTable.Rows[0]["Date_Time"]);
            _dateTime = _dateTimeUtc.ToLocalTime();
            _durationDay = (decimal) (dataTable.Rows[0]["Duration_Day"]);
            _duration = (int) (_durationDay / SecondToDay);
            _endDateTimeUtc = _dateTimeUtc.AddSeconds(_duration);
            int isPublic = (int) (short) (dataTable.Rows[0]["Is_Public"]);
            _isPublic = (isPublic == 1);

            Topic = (cachedTopic?.TopicID == topicID) ? cachedTopic : new Topic(topicID);

            dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Topic.TopicTableID + ", Owner_ID", Topic.TopicTable, Topic.TopicTableID + " = :topicID"),
                new CommandParameter(":topicID", Topic.TopicID));
            int ownerID = (int) (decimal) (dataTable.Rows[0]["Owner_ID"]);
            Owner = (UserAuthentication.LoggedInUser?.UserID == ownerID)
                ? UserAuthentication.LoggedInUser
                : new User(ownerID);

            dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                QuizQuestionTableID + ", Question_ID", QuizQuestionTable,
                "Quiz_ID = :quizID", orderByClause: "Question_Order"),
                new CommandParameter(":quizID", QuizID));

            _quizQuestionIDList = new List<int>(dataTable.Rows.Count);
            _questionList = new List<Question.Question>(dataTable.Rows.Count);
            foreach (DataRow row in dataTable.Rows) {
                _quizQuestionIDList.Add((int) (decimal) (row[QuizQuestionTableID]));
                _questionList.Add(new Question.Question(((int) (decimal) (row["Question_ID"])), Topic));
            }
        }


        public override bool Equals(object obj) {
            return Equals(obj as Quiz);
        }

        public bool Equals(Quiz quiz) {
            if ((object) quiz == null) return false;
            return (this.QuizID == quiz.QuizID);
        }

        public override int GetHashCode() {
            return this.QuizID.GetHashCode();
        }

        public static bool operator ==(Quiz quiz1, Quiz quiz2) {
            if (object.ReferenceEquals(quiz1, quiz2)) return true;
            if (((object) quiz1 == null) || ((object) quiz2 == null)) return false;
            return (quiz1.QuizID == quiz2.QuizID);
        }

        public static bool operator !=(Quiz quiz1, Quiz quiz2) {
            return !(quiz1 == quiz2);
        }
    }
}