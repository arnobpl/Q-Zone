using System;
using System.Data;
using System.Management.Instrumentation;
using System.Security.Authentication;
using Q_Zone.Models.Account;
using Q_Zone.Models.DataConnector;

// ReSharper disable RedundantNameQualifier
// ReSharper disable ArrangeThisQualifier

namespace Q_Zone.Models.Quiz
{
    public class Result
    {
        public const string ResultTable = "Quiz_Result";
        public const string ResultTableID = "Result_ID";

        public int ResultID { get; }

        public User Owner { get; }

        public Quiz Quiz { get; }

        public int ObtainedMarks { get; }

        public int PercentageOfObtainedMarks { get; }

        /// <summary>
        /// Checks if logged-in user is the owner
        /// </summary>
        /// <returns>ture if logged-in user is the owner, otherwise false</returns>
        public bool IsLoggedInUserOwner() {
            return (this.Owner == UserAuthentication.LoggedInUser);
        }

        /// <summary>
        /// This is used for viewing logged-in user's result for the given quiz
        /// </summary>
        /// <param name="quiz">quiz to view its result for logged-in user</param>
        public Result(Quiz quiz) {
            if ((object) quiz == null) {
                throw new ArgumentException("Topic cannot be null", nameof(quiz));
            }

            if ((object) UserAuthentication.LoggedInUser == null) {
                throw new InvalidCredentialException("User must be logged in before viewing result by quiz");
            }
            Owner = UserAuthentication.LoggedInUser;
            Quiz = quiz;

            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                ResultTableID + ", Obtained_Marks, Percentage", ResultTable, "Owner_ID = :ownerID AND Quiz_ID = :quizID"),
                new CommandParameter(":ownerID", Owner.UserID),
                new CommandParameter(":quizID", Quiz.QuizID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("Result of the logged-in user for the quiz does not exist");
            }

            ResultID = (int) (decimal) (dataTable.Rows[0][ResultTableID]);
            ObtainedMarks = (int) (decimal) (dataTable.Rows[0]["Obtained_Marks"]);
            PercentageOfObtainedMarks = (int) (decimal) (dataTable.Rows[0]["Percentage"]);
        }

        /// <summary>
        /// This is used for viewing any result by resultID
        /// </summary>
        /// <param name="resultID">resultID to view result</param>
        /// <param name="cachedQuiz">cached quiz for optimization purpose by preventing multiple creations of same quiz</param>
        public Result(int resultID, Quiz cachedQuiz = null) {
            DataTable dataTable = DataAccessLayer.SelectCommand(DataAccessLayer.SelectCommandString(
                "Owner_ID, Quiz_ID, Obtained_Marks, Percentage", ResultTable, ResultTableID + " = :resultID"),
                new CommandParameter(":resultID", resultID));

            if (dataTable.Rows.Count != 1) {
                throw new InstanceNotFoundException("ResultID does not exist");
            }
            ResultID = resultID;

            int ownerID = (int) (decimal) (dataTable.Rows[0]["Owner_ID"]);
            Owner = (UserAuthentication.LoggedInUser?.UserID == ownerID)
                ? UserAuthentication.LoggedInUser
                : new User(ownerID);

            int quizID = (int) (decimal) (dataTable.Rows[0]["Quiz_ID"]);
            Quiz = (cachedQuiz?.QuizID == quizID) ? cachedQuiz : new Quiz(quizID);

            ObtainedMarks = (int) (decimal) (dataTable.Rows[0]["Obtained_Marks"]);
            PercentageOfObtainedMarks = (int) (decimal) (dataTable.Rows[0]["Percentage"]);
        }


        public override bool Equals(object obj) {
            return Equals(obj as Result);
        }

        public bool Equals(Result result) {
            if ((object) result == null) return false;
            return (this.ResultID == result.ResultID);
        }

        public override int GetHashCode() {
            return this.ResultID.GetHashCode();
        }

        public static bool operator ==(Result result1, Result result2) {
            if (object.ReferenceEquals(result1, result2)) return true;
            if (((object) result1 == null) || ((object) result2 == null)) return false;
            return (result1.ResultID == result2.ResultID);
        }

        public static bool operator !=(Result result1, Result result2) {
            return !(result1 == result2);
        }
    }
}