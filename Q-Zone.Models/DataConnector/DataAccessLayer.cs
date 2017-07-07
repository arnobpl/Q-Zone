using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Linq;
using Oracle.ManagedDataAccess.Client;

namespace Q_Zone.Models.DataConnector
{
    /// <summary>
    /// Used as parameter for database command
    /// </summary>
    public class CommandParameter
    {
        public string Name;
        public object Value;

        public CommandParameter(string name, object value) {
            Name = name;
            Value = value;
        }
    }

    /// <summary>
    /// This class is responsible for all the database interactions.
    /// The connection string is defined in 'Web.config' file.
    /// This class only supports Oracle Database connection using 'Oracle.ManagedDataAccess.Client' provider.
    /// </summary>
    public static class DataAccessLayer
    {
        /// <summary>
        /// Get connection string defined in 'Web.config' file
        /// </summary>
        public static string ConnectionString { get; private set; }

        private static string _databaseConnectionName;
        /// <summary>
        /// Get or set the database connection name string defined in 'Web.config' file
        /// </summary>
        public static string DatabaseConnectionName {
            get { return _databaseConnectionName; }
            set {
                _databaseConnectionName = value;
                //TODO: solve the issue of "Web.config" (currently just workaround)
                //ConnectionString = ConfigurationManager.ConnectionStrings[_databaseConnectionName].ToString();
                ConnectionString = "Data Source=PDBORCL;User Id=Q_Zone;Password=1;";
            }
        }

        static DataAccessLayer() {
            // set default database connection name string
            DatabaseConnectionName = "DefaultConnection";
        }

        #region 'SELECT' command

        /// <summary>
        /// String for 'SELECT' command from shorthand parameters
        /// </summary>
        /// <param name="selectClause">string for 'SELECT' clause</param>
        /// <param name="fromClause">string for 'FROM' clause</param>
        /// <param name="whereClause">string for 'WHERE' clause</param>
        /// <param name="groupByClause">string for 'GROUP BY' clause</param>
        /// <param name="havingClause">string for 'HAVING' clause</param>
        /// <param name="orderByClause">string for 'ORDER BY' clause</param>
        /// <returns>string for 'SELECT' command</returns>
        public static string SelectCommandString(string selectClause = "*", string fromClause = "Dual",
            string whereClause = "", string groupByClause = "", string havingClause = "", string orderByClause = "") {
            // set initial command string
            string commandString = "SELECT " + selectClause + " FROM " + fromClause;

            if (whereClause != "") commandString += " WHERE " + whereClause;
            if (groupByClause != "") {
                commandString += " GROUP BY " + groupByClause;
                if (havingClause != "") commandString += " HAVING " + havingClause;
            }
            if (orderByClause != "") commandString += " ORDER BY " + orderByClause;

            return commandString;
        }

        /// <summary>
        /// 'SELECT' command with optional parameters
        /// </summary>
        /// <param name="commandString">String (with optional':paramName') for 'SELECT' command (without semicolon)</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>data of 'DataTable' type if succcess, otherwise null</returns>
        public static DataTable SelectCommand(string commandString, params CommandParameter[] parameters) {
            using (OracleConnection connection = new OracleConnection(ConnectionString)) {
                using (OracleCommand command = new OracleCommand(commandString, connection)) {
                    using (OracleDataAdapter dataAdapter = new OracleDataAdapter()) {
                        Debug.WriteLine("Executing 'SELECT' command for: \"" + commandString + "\"");

                        dataAdapter.SelectCommand = command;

                        foreach (CommandParameter param in parameters) {
                            dataAdapter.SelectCommand.Parameters.Add(param.Name, param.Value);
                        }

                        try {
                            dataAdapter.SelectCommand.Connection.Open();
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return null;
                        }

                        DataTable dataTable = new DataTable();
                        try {
                            dataAdapter.Fill(dataTable);
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return null;
                        }

                        try {
                            dataAdapter.SelectCommand.Connection.Close();
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                        }

                        return dataTable;
                    }
                }
            }
        }

        #endregion

        #region 'INSERT' command

        /// <summary>
        /// Column name for ID
        /// </summary>
        private const string ColumnNameID = "ID";

        /// <summary>
        /// Generates command string to generate unique value for 'columnNameID' column
        /// </summary>
        /// <param name="columnNameID"></param>
        /// <returns>command string to generate unique value for 'columnNameID' column</returns>
        private static string CommandGenerateID(string columnNameID) {
            return "(NVL(MAX(" + columnNameID + "), 0) + 1)";
        }

        /// <summary>
        /// Value parameter for ID column
        /// </summary>
        private const string ValueParameterForID = ":uniqueAutoID";

        /// <summary>
        /// String to generate unique value for 'columnNameID' column of the given table
        /// </summary>
        /// <param name="tableName">table name</param>
        /// <param name="columnNameID">column name for generating unique ID</param>
        /// <returns>unique ID for 'columnNameID' column of the given table if success, otherwise -1</returns>
        public static int GenerateID(string tableName, string columnNameID = ColumnNameID) {
            DataTable dataTable = SelectCommand(SelectCommandString(CommandGenerateID(columnNameID), tableName));
            if (dataTable.Rows.Count != 1) return -1;
            return ((int) (decimal) (dataTable.Rows[0][0]));
        }

        /// <summary>
        /// 'INSERT' command for all columns with auto-generated ID.
        /// Note that it internally uses a parameter named 'uniqueAutoID'.
        /// </summary>
        /// <param name="insertID">out parameter for inserted ID</param>
        /// <param name="insertIntoClause">string for 'INSERT INTO' clause</param>
        /// <param name="valuesClause">string for 'VALUES' clause</param>
        /// <param name="columnNameID">column name for generating ID</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int InsertCommand_AllColumnAutoID(out int insertID, string insertIntoClause, string valuesClause,
            string columnNameID = ColumnNameID, params CommandParameter[] parameters) {
            return InsertCommand_SpecificColumnAutoID(out insertID, insertIntoClause, "", valuesClause, columnNameID,
                parameters);
        }

        /// <summary>
        /// 'INSERT' command for all columns with auto-generated ID.
        /// Note that it internally uses a parameter named 'uniqueAutoID'.
        /// </summary>
        /// <param name="insertIntoClause">string for 'INSERT INTO' clause</param>
        /// <param name="valuesClause">string for 'VALUES' clause</param>
        /// <param name="columnNameID">column name for generating ID</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int InsertCommand_AllColumnAutoID(string insertIntoClause, string valuesClause,
            string columnNameID = ColumnNameID, params CommandParameter[] parameters) {
            int insertID;
            return InsertCommand_SpecificColumnAutoID(out insertID, insertIntoClause, "", valuesClause, columnNameID,
                parameters);
        }

        /// <summary>
        /// 'INSERT' command for specific columns with auto-generated ID.
        /// Note that it internally uses a parameter named 'uniqueAutoID'.
        /// </summary>
        /// <param name="insertID">out parameter for inserted ID</param>
        /// <param name="insertIntoClause">string for 'INSERT INTO' clause</param>
        /// <param name="columnClause">string for specific column clause</param>
        /// <param name="valuesClause">string for 'VALUES' clause</param>
        /// <param name="columnNameID">column name for generating unique ID</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int InsertCommand_SpecificColumnAutoID(out int insertID, string insertIntoClause,
            string columnClause, string valuesClause, string columnNameID = ColumnNameID,
            params CommandParameter[] parameters) {
            if (columnClause != "") columnClause = columnNameID + ", " + columnClause;
            valuesClause = ValueParameterForID + ", " + valuesClause;

            lock ("Inserting with auto-generated ID into table: " + insertIntoClause) {
                insertID = GenerateID(insertIntoClause, columnNameID);
                List<CommandParameter> parameterList = parameters.ToList();
                parameterList.Insert(0, new CommandParameter(ValueParameterForID, insertID));

                return InsertCommand(InsertCommandString(insertIntoClause, columnClause, valuesClause),
                    parameterList.ToArray());
            }
        }

        /// <summary>
        /// 'INSERT' command for specific columns with auto-generated ID.
        /// Note that it internally uses a parameter named 'uniqueAutoID'.
        /// </summary>
        /// <param name="insertIntoClause">string for 'INSERT INTO' clause</param>
        /// <param name="columnClause">string for specific column clause</param>
        /// <param name="valuesClause">string for 'VALUES' clause</param>
        /// <param name="columnNameID">column name for generating unique ID</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int InsertCommand_SpecificColumnAutoID(string insertIntoClause, string columnClause,
            string valuesClause, string columnNameID = ColumnNameID, params CommandParameter[] parameters) {
            int insertID;
            return InsertCommand_SpecificColumnAutoID(out insertID, insertIntoClause, columnClause, valuesClause,
                columnNameID, parameters);
        }

        /// <summary>
        /// String for 'INSERT' command from shorthand parameters
        /// </summary>
        /// <param name="insertIntoClause">string for 'INSERT INTO' clause</param>
        /// <param name="columnClause">string for specific column clause</param>
        /// <param name="valuesClause">string for 'VALUES' clause</param>
        /// <returns>string for 'INSERT' command</returns>
        public static string InsertCommandString(string insertIntoClause, string columnClause = "",
            string valuesClause = "NULL") {
            string commandString = "INSERT INTO " + insertIntoClause;
            if (columnClause != "") commandString += " (" + columnClause + ")";
            commandString += " VALUES (" + valuesClause + ")";
            return commandString;
        }

        /// <summary>
        /// 'INSERT' command with optional parameters
        /// </summary>
        /// <param name="commandString">String (with optional ':paramName') for 'INSERT' command (without semicolon)</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int InsertCommand(string commandString, params CommandParameter[] parameters) {
            using (OracleConnection connection = new OracleConnection(ConnectionString)) {
                using (OracleCommand command = new OracleCommand(commandString, connection)) {
                    using (OracleDataAdapter dataAdapter = new OracleDataAdapter()) {
                        Debug.WriteLine("Executing 'INSERT' command for: \"" + commandString + "\"");

                        dataAdapter.InsertCommand = command;

                        foreach (CommandParameter param in parameters) {
                            dataAdapter.InsertCommand.Parameters.Add(param.Name, param.Value);
                        }

                        try {
                            dataAdapter.InsertCommand.Connection.Open();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.InsertCommand.ExecuteNonQuery();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.InsertCommand.Connection.Close();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        return 0;
                    }
                }
            }
        }

        #endregion

        #region 'UPDATE' command

        /// <summary>
        /// String for 'UPDATE' command from shorthand parameters
        /// </summary>
        /// <param name="updateClause">string for 'UPDATE' clause</param>
        /// <param name="setClause">string for 'SET' clause</param>
        /// <param name="whereClause">string for 'WHERE' clause</param>
        /// <returns>string for 'UPDATE' command</returns>
        public static string UpdateCommandString(string updateClause, string setClause, string whereClause = "") {
            string commandString = "UPDATE " + updateClause + " SET " + setClause;
            if (whereClause != "") commandString += " WHERE " + whereClause;
            return commandString;
        }

        /// <summary>
        /// 'UPDATE' command with optional parameters
        /// </summary>
        /// <param name="commandString">String (with optional ':paramName') for 'UPDATE' command (without semicolon)</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int UpdateCommand(string commandString, params CommandParameter[] parameters) {
            using (OracleConnection connection = new OracleConnection(ConnectionString)) {
                using (OracleCommand command = new OracleCommand(commandString, connection)) {
                    using (OracleDataAdapter dataAdapter = new OracleDataAdapter()) {
                        Debug.WriteLine("Executing 'UPDATE' command for: \"" + commandString + "\"");

                        dataAdapter.UpdateCommand = command;

                        foreach (CommandParameter param in parameters) {
                            dataAdapter.UpdateCommand.Parameters.Add(param.Name, param.Value);
                        }

                        try {
                            dataAdapter.UpdateCommand.Connection.Open();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.UpdateCommand.ExecuteNonQuery();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.UpdateCommand.Connection.Close();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        return 0;
                    }
                }
            }
        }

        #endregion

        #region 'DELETE' command

        /// <summary>
        /// String for 'DELETE' command from shorthand parameters
        /// </summary>
        /// <param name="deleteFromClause">string for 'DELETE FROM' clause</param>
        /// <param name="whereClause">string for 'WHERE' clause</param>
        /// <returns>string for 'DELETE' command</returns>
        public static string DeleteCommandString(string deleteFromClause, string whereClause = "") {
            string commandString = "DELETE " + deleteFromClause;
            if (whereClause != "") commandString += " WHERE " + whereClause;
            return commandString;
        }

        /// <summary>
        /// 'DELETE' command with optional parameters
        /// </summary>
        /// <param name="commandString">String (with optional ':paramName') for 'DELETE' command (without semicolon)</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int DeleteCommand(string commandString, params CommandParameter[] parameters) {
            using (OracleConnection connection = new OracleConnection(ConnectionString)) {
                using (OracleCommand command = new OracleCommand(commandString, connection)) {
                    using (OracleDataAdapter dataAdapter = new OracleDataAdapter()) {
                        Debug.WriteLine("Executing 'DELETE' command for: \"" + commandString + "\"");

                        dataAdapter.DeleteCommand = command;

                        foreach (CommandParameter param in parameters) {
                            dataAdapter.DeleteCommand.Parameters.Add(param.Name, param.Value);
                        }

                        try {
                            dataAdapter.DeleteCommand.Connection.Open();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.DeleteCommand.ExecuteNonQuery();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.DeleteCommand.Connection.Close();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        return 0;
                    }
                }
            }
        }

        #endregion

        #region 'PROCEDURE' command

        /// <summary>
        /// 'PROCEDURE' command with optional parameters
        /// </summary>
        /// <param name="procedureName">Procedure name</param>
        /// <param name="parameters">Parameter list with name and value</param>
        /// <returns>0 if succcess, otherwise the error code</returns>
        public static int ProcedureCommand(string procedureName, params CommandParameter[] parameters) {
            using (OracleConnection connection = new OracleConnection(ConnectionString)) {
                using (OracleCommand command = new OracleCommand(procedureName, connection)) {
                    using (OracleDataAdapter dataAdapter = new OracleDataAdapter()) {
                        Debug.WriteLine("Executing 'PROCEDURE' command for: \"" + procedureName + "\"");

                        dataAdapter.SelectCommand = command;
                        dataAdapter.SelectCommand.CommandType = CommandType.StoredProcedure;

                        foreach (CommandParameter param in parameters) {
                            dataAdapter.SelectCommand.Parameters.Add(param.Name, param.Value);
                        }

                        try {
                            dataAdapter.SelectCommand.Connection.Open();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.SelectCommand.ExecuteNonQuery();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        try {
                            dataAdapter.SelectCommand.Connection.Close();
                        }
                        catch (OracleException exception) {
                            Debug.WriteLine(exception.Message);
                            return exception.Number;
                        }
                        catch (Exception exception) {
                            Debug.WriteLine(exception.Message);
                            return -1;
                        }

                        return 0;
                    }
                }
            }
        }

        #endregion
    }
}