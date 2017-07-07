using System;
using System.Collections.Generic;
using System.Data;
using System.Security;
using System.Security.Authentication;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;

// ReSharper disable ArrangeThisQualifier

namespace Q_Zone.Models.Quiz
{
    public class AnswerSheet
    {
        private const string AnswerSheetTable = "Answer_Sheet";
        private const string AnswerSheetTableID = "Question_Answer_ID";

        public User Owner { get; }

        public Quiz Quiz { get; }

        private int _resultID;

        public bool IsSubmitted { get; private set; }

        private readonly List<Question.Question> _quizQuestionList;

        private readonly List<int> _toBeSubmittedQuestionIDList;
        private readonly List<int> _toBeSubmittedAnswerOptionIDList;

        /// <summary>
        /// Adds or replaces questionID and optionID to the pending submission list
        /// </summary>
        /// <param name="questionID">questionID</param>
        /// <param name="optionID">optionID</param>
        private void AddOrReplaceToPendingSubmissionList(int questionID, int optionID) {
            int findIndex = _toBeSubmittedQuestionIDList.IndexOf(questionID);
            if (findIndex == -1) {
                _toBeSubmittedQuestionIDList.Add(questionID);
                _toBeSubmittedAnswerOptionIDList.Add(optionID);
            }
            else {
                _toBeSubmittedAnswerOptionIDList[findIndex] = optionID;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="questionID">questionID</param>
        private void RemoveFromPendingSubmissionList(int questionID) {
            int findIndex = _toBeSubmittedQuestionIDList.IndexOf(questionID);
            if (findIndex != -1) {
                _toBeSubmittedQuestionIDList.RemoveAt(findIndex);
                _toBeSubmittedAnswerOptionIDList.RemoveAt(findIndex);
            }
        }

        /// <summary>
        /// This method temporarily saves the given answer to the question of the quiz.
        /// It does not save the given answer in the database until "Submit()" is called.
        /// </summary>
        /// <param name="question">question to answer</param>
        /// <param name="optionString">option string of the question to answer, or null to clear already the given answer of the question</param>
        /// <returns>true if success, otherwise false</returns>
        public bool GiveAnswer(Question.Question question, string optionString) {
            if (!IsAnswerSheetEditableForLoggedInUser()) return false;
            if ((object) question == null) return false;
            if (_quizQuestionList.IndexOf(question) == -1) return false;

            if (string.IsNullOrWhiteSpace(optionString)) {
                RemoveFromPendingSubmissionList(question.QuestionID);
                return true;
            }

            int optionID = question.GetOptionID(optionString);
            if (optionID == -1) return false;
            AddOrReplaceToPendingSubmissionList(question.QuestionID, optionID);
            return true;
        }

        /// <summary>
        /// Shows given answer of submitted answer sheet
        /// </summary>
        /// <param name="question"></param>
        /// <returns>given answer if success, otherwise null</returns>
        public string ShowGivenAnswer(Question.Question question) {
            if (!IsSubmitted) return null;
            if ((object) question == null) return null;
            if (_quizQuestionList.IndexOf(question) == -1) return null;

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Option_ID", AnswerSheetTable, "Result_ID = :resultID AND Question_ID = :questionID"),
                new CommandParameter(":resultID", _resultID),
                new CommandParameter(":questionID", question.QuestionID));
            if (dataTable.Rows.Count != 1) return null;
            int optionID = (int) (decimal) (dataTable.Rows[0][0]);
            return question.GetAnswerOptionByOptionID(optionID);
        }

        /// <summary>
        /// This method submits the answer sheet.
        /// It must be called while the quiz is running.
        /// There is no re-submission for any quiz.
        /// </summary>
        /// <returns>true if success, otherwise false</returns>
        public bool Submit() {
            if (!IsAnswerSheetEditableForLoggedInUser()) return false;

            int returnValue = DataAccessLayer.InsertCommand_SpecificColumnAutoID(out _resultID, Result.ResultTable,
                "Owner_ID, Quiz_ID", ":ownerID, :quizID", Result.ResultTableID,
                new CommandParameter(":ownerID", Owner.UserID),
                new CommandParameter(":quizID", Quiz.QuizID));
            if (returnValue != 0) return false;

            int toBeSubmittedItems = _toBeSubmittedQuestionIDList.Count;
            int successfullySubmittedItems = 0;
            for (int i = 0; i < toBeSubmittedItems; i++) {
                returnValue = DataAccessLayer.InsertCommand_AllColumnAutoID(AnswerSheetTable,
                    ":resultID, :questionID, :optionID", AnswerSheetTableID,
                    new CommandParameter(":resultID", _resultID),
                    new CommandParameter(":questionID", _toBeSubmittedQuestionIDList[i]),
                    new CommandParameter(":optionID", _toBeSubmittedAnswerOptionIDList[i]));
                if (returnValue == 0) successfullySubmittedItems++;
            }

            IsSubmitted = true;
            _toBeSubmittedQuestionIDList.Clear();
            _toBeSubmittedAnswerOptionIDList.Clear();

            EvaluateAndUpdateQuizStatistics();

            return (toBeSubmittedItems == successfullySubmittedItems);
        }

        /// <summary>
        /// Evaluates submitted answer sheet after submission and updates quiz statistics
        /// </summary>
        private void EvaluateAndUpdateQuizStatistics() {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Question_ID, Option_ID", AnswerSheetTable, "Result_ID = :resultID"),
                new CommandParameter(":resultID", _resultID));

            int givenCorrectAnswers = 0;
            int givenWrongAnswers = 0;
            foreach (DataRow row in dataTable.Rows) {
                int questionID = (int) (decimal) (row["Question_ID"]);
                int givenOptionID = (int) (decimal) (row["Option_ID"]);
                int currectAnswerOptionID = CorrectAnswerOptionID(questionID);
                if (givenOptionID == currectAnswerOptionID) givenCorrectAnswers++;
                else givenWrongAnswers++;
            }

            int obtainedMarks = ((givenCorrectAnswers * Quiz.EachQuestionMarks)
                                 - (givenWrongAnswers * Quiz.EachQuestionMinusMarks));
            int percentageOfObtainedMarks = (int) Math.Round((((double) obtainedMarks) / Quiz.TotalMarks) * 100);

            DataAccessLayer.UpdateCommand(DataAccessLayer.UpdateCommandString(Result.ResultTable,
                "Obtained_Marks = :obtainedMarks, Percentage = :percentage", Result.ResultTableID + " = :resultID"),
                new CommandParameter(":obtainedMarks", obtainedMarks),
                new CommandParameter(":percentage", percentageOfObtainedMarks),
                new CommandParameter(":resultID", _resultID));

            int currentTotalParticipants = Quiz.TotalParticipants;
            int currentAverageMarks = Quiz.AverageMarks;

            int updatedTotalParticipants = (currentTotalParticipants + 1);
            int updatedAverageMarks = (int) Math.Round(
                ((currentAverageMarks * currentTotalParticipants) + obtainedMarks) /
                ((double) updatedTotalParticipants));

            DataAccessLayer.UpdateCommand(
                DataAccessLayer.UpdateCommandString(Quiz.QuizTable,
                    "Total_Participants = :totalParticipants, Average_Marks = :averageMarks",
                    Quiz.QuizTableID + " = :quizID"),
                new CommandParameter(":totalParticipants", updatedTotalParticipants),
                new CommandParameter(":averageMarks", updatedAverageMarks),
                new CommandParameter(":quizID", Quiz.QuizID));
        }

        /// <summary>
        /// Gets correct answer's optionID of a question
        /// </summary>
        /// <param name="questionID">questionID of a question</param>
        /// <returns>correct answer's optionID of the given question</returns>
        private int CorrectAnswerOptionID(int questionID) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Correct_Answer_ID", Question.Question.QuestionTable,
                Question.Question.QuestionTableID + " = :questionID"),
                new CommandParameter(":questionID", questionID));
            return ((int) (decimal) (dataTable.Rows[0][0]));
        }

        /// <summary>
        /// Checks if this answer sheet is editable for logged-in user
        /// </summary>
        /// <returns>true if editable, otherwise false</returns>
        private bool IsAnswerSheetEditableForLoggedInUser() {
            return (IsLoggedInUserOwner() && !IsSubmitted && Quiz.IsQuizRunning());
        }

        /// <summary>
        /// Checks if logged-in user is the owner
        /// </summary>
        /// <returns>ture if logged-in user is the owner, otherwise false</returns>
        public bool IsLoggedInUserOwner() {
            return (this.Owner == UserAuthentication.LoggedInUser);
        }

        /// <summary>
        /// Creates or shows the answer sheet of the quiz for the logged-in user
        /// </summary>
        /// <param name="quiz">answer sheet's quiz</param>
        public AnswerSheet(Quiz quiz) {
            if ((object) quiz == null) {
                throw new ArgumentException("Quiz cannot be null", nameof(quiz));
            }

            if (!quiz.IsPublic) {
                throw new SecurityException("Quiz is not public yet");
            }

            if ((object) UserAuthentication.LoggedInUser == null) {
                throw new InvalidCredentialException("User must be logged in before creating a answer sheet");
            }
            Owner = UserAuthentication.LoggedInUser;
            Quiz = quiz;

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                Result.ResultTableID, Result.ResultTable, "Owner_ID = :ownerID AND Quiz_ID = :quizID"),
                new CommandParameter(":ownerID", Owner.UserID),
                new CommandParameter(":quizID", quiz.QuizID));

            if (dataTable.Rows.Count != 1) {
                IsSubmitted = false;
                if (!quiz.IsQuizFinished()) {
                    _toBeSubmittedQuestionIDList = new List<int>(quiz.TotalQuestions);
                    _toBeSubmittedAnswerOptionIDList = new List<int>(quiz.TotalQuestions);
                }
            }
            else {
                IsSubmitted = true;
                _resultID = (int) (decimal) (dataTable.Rows[0][0]);
            }

            _quizQuestionList = quiz.ViewQuestions();
        }
    }
}