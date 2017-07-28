using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace PRISM
{

    /// <summary>
    /// Tools to retrieve data from a database
    /// </summary>
    public class clsDBTools : clsEventNotifier
    {

        #region "Constants"

        /// <summary>
        /// Default timeout length, in seconds, when waiting for a query to finish running
        /// </summary>
        public const int DEFAULT_SP_TIMEOUT_SEC = 30;

        #endregion

        #region "Member Variables"

        /// <summary>
        /// Timeout length, in seconds, when waiting for a query to finish running
        /// </summary>
        private int mTimeoutSeconds;

        #endregion

        #region "Properties"

        /// <summary>
        /// Database connection string.
        /// </summary>
        public string ConnectStr { get; set; }

        /// <summary>
        /// Timeout length, in seconds, when waiting for a query to finish executing
        /// </summary>
        public int TimeoutSeconds
        {
            get => mTimeoutSeconds;
            set
            {
                if (value == 0)
                    value = DEFAULT_SP_TIMEOUT_SEC;

                if (value < 10)
                    value = 10;

                mTimeoutSeconds = value;
            }
        }

        #endregion

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="connectionString">Database connection string</param>
        public clsDBTools(string connectionString)
        {
            ConnectStr = connectionString;
        }

        /// <summary>
        /// Get a mapping from column name to column index, based on column order
        /// </summary>
        /// <param name="columns"></param>
        /// <returns>Mapping from column name to column index</returns>
        /// <remarks>Use in conjunction with GetColumnValue, e.g. GetColumnValue(resultRow, columnMap, "ID")</remarks>
        public Dictionary<string, int> GetColumnMapping(IReadOnlyList<string> columns)
        {

            var columnMap = new Dictionary<string, int>();

            for (var i = 0; i < columns.Count; i++)
            {
                columnMap.Add(columns[i], i);
            }

            return columnMap;
        }

        /// <summary>
        /// Get the string value for the specified column
        /// </summary>
        /// <param name="resultRow">Row of results, as returned by GetQueryResults</param>
        /// <param name="columnMap">Map of column name to column index, as returned by GetColumnMapping</param>
        /// <param name="columnName">Column Name</param>
        /// <returns>String value</returns>
        /// <remarks>The returned value could be null, but note that GetQueryResults converts all Null strings to string.empty</remarks>
        public string GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName)
        {
            if (!columnMap.TryGetValue(columnName, out var columnIndex))
                throw new Exception("Invalid column name: " + columnName);

            var value = resultRow[columnIndex];

            return value;
        }

        /// <summary>
        /// Get the integer value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        public int GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName,
            int defaultValue)
        {
            return GetColumnValue(resultRow, columnMap, columnName, defaultValue, out _);
        }

        /// <summary>
        /// Get the integer value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        /// <param name="resultRow">Row of results, as returned by GetQueryResults</param>
        /// <param name="columnMap">Map of column name to column index, as returned by GetColumnMapping</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="validNumber">Output: set to true if the column contains an integer</param>
        /// <returns>Integer value</returns>
        public int GetColumnValue(
        IReadOnlyList<string> resultRow,
        IReadOnlyDictionary<string, int> columnMap,
        string columnName,
        int defaultValue,
        out bool validNumber)
        {
            if (!columnMap.TryGetValue(columnName, out var columnIndex))
                throw new Exception("Invalid column name: " + columnName);

            var valueText = resultRow[columnIndex];

            if (int.TryParse(valueText, out var value))
            {
                validNumber = true;
                return value;
            }

            validNumber = false;
            return defaultValue;
        }

        /// <summary>
        /// Get the double value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        public double GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName,
            double defaultValue)
        {
            return GetColumnValue(resultRow, columnMap, columnName, defaultValue, out _);
        }

        /// <summary>
        /// Get the double value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        /// <param name="resultRow">Row of results, as returned by GetQueryResults</param>
        /// <param name="columnMap">Map of column name to column index, as returned by GetColumnMapping</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="validNumber">Output: set to true if the column contains a double (or integer)</param>
        /// <returns>Double value</returns>
        public double GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName,
            double defaultValue,
            out bool validNumber)
        {
            if (!columnMap.TryGetValue(columnName, out var columnIndex))
                throw new Exception("Invalid column name: " + columnName);

            var valueText = resultRow[columnIndex];

            if (double.TryParse(valueText, out var value))
            {
                validNumber = true;
                return value;
            }

            validNumber = false;
            return defaultValue;
        }

        /// <summary>
        /// Get the date value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        public DateTime GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName,
            DateTime defaultValue)
        {
            return GetColumnValue(resultRow, columnMap, columnName, defaultValue, out _);
        }

        /// <summary>
        /// Get the date value for the specified column, or the default value if the value is empty or non-numeric
        /// </summary>
        /// <param name="resultRow">Row of results, as returned by GetQueryResults</param>
        /// <param name="columnMap">Map of column name to column index, as returned by GetColumnMapping</param>
        /// <param name="columnName">Column Name</param>
        /// <param name="defaultValue">Default value</param>
        /// <param name="validNumber">Output: set to true if the column contains a valid date</param>
        /// <returns>True or false</returns>
        public DateTime GetColumnValue(
            IReadOnlyList<string> resultRow,
            IReadOnlyDictionary<string, int> columnMap,
            string columnName,
            DateTime defaultValue,
            out bool validNumber)
        {
            if (!columnMap.TryGetValue(columnName, out var columnIndex))
                throw new Exception("Invalid column name: " + columnName);

            var valueText = resultRow[columnIndex];

            if (DateTime.TryParse(valueText, out var value))
            {
                validNumber = true;
                return value;
            }

            validNumber = false;
            return defaultValue;
        }

        /// <summary>
        /// The subroutine is an event handler for InfoMessage event.
        /// </summary>
        /// <remarks>
        /// The errors and warnings sent from the SQL server are caught here
        /// </remarks>
        private void OnInfoMessage(object sender, SqlInfoMessageEventArgs args)
        {
            foreach (SqlError err in args.Errors)
            {
                var s = "";
                s += "Message: " + err.Message;
                s += ", Source: " + err.Source;
                s += ", Class: " + err.Class;
                s += ", State: " + err.State;
                s += ", Number: " + err.Number;
                s += ", LineNumber: " + err.LineNumber;
                s += ", Procedure:" + err.Procedure;
                s += ", Server: " + err.Server;
                OnErrorEvent(s);
            }
        }

#if !(NETSTANDARD1_x || NETSTANDARD2_0)
        /// <summary>
        /// The function gets a disconnected dataset as specified by the SQL statement.
        /// </summary>
        /// <param name="sqlQuery">A SQL string.</param>
        /// <param name="DS">A dataset.</param>
        /// <param name="rowCount">A row counter.</param>
        /// <return>Returns a disconnected dataset as specified by the SQL statement.</return>
        [Obsolete("Use GetQueryResults since support for DataSet objects is dropped in .NET Standard")]
        public bool GetDiscDataSet(string sqlQuery, ref DataSet DS, ref int rowCount)
        {
            var retryCount = 3;
            var retryDelaySeconds = 5;

            while (retryCount > 0)
            {
                try
                {
                    using (var dbConnection = new SqlConnection(ConnectStr))
                    {
                        dbConnection.InfoMessage += OnInfoMessage;

                        // Get the dataset
                        var adapter = new SqlDataAdapter(sqlQuery, dbConnection);
                        DS = new DataSet();
                        rowCount = adapter.Fill(DS);
                        return true;

                    }

                }
                catch (Exception ex)
                {
                    retryCount -= 1;
                    var errorMessage =
                        string.Format("Exception querying database ({0}; " + "ConnectionString: {1}, RetryCount = {2}, Query {3}",
                                      ex.Message, ConnectStr, retryCount, sqlQuery);

                    OnErrorEvent(errorMessage);

                    // Delay for 5 seconds before trying again
                    clsProgRunner.SleepMilliseconds(retryDelaySeconds * 1000);

                }
            }

            return false;

        }
#endif

        /// <summary>
        /// Run a query against a SQL Server database, return the results as a list of strings
        /// </summary>
        /// <param name="sqlQuery">Query to run</param>
        /// <param name="lstResults">Results (list of list of strings)</param>
        /// <param name="callingFunction">Name of the calling function (for logging purposes)</param>
        /// <param name="retryCount">Number of times to retry (in case of a problem)</param>
        /// <param name="maxRowsToReturn">Maximum rows to return; 0 to return all rows</param>
        /// <param name="retryDelaySeconds">Number of seconds to wait between retrying the call to the procedure</param>
        /// <returns>True if success, false if an error</returns>
        /// <remarks>
        /// Uses the connection string passed to the constructor of this class
        /// Null values are converted to empty strings
        /// Numbers are converted to their string equivalent
        /// By default, retries the query up to 3 times
        /// </remarks>
        public bool GetQueryResults(
            string sqlQuery,
            out List<List<string>> lstResults,
            string callingFunction,
            short retryCount = 3,
            int maxRowsToReturn = 0,
            int retryDelaySeconds = 5)
        {

            if (retryCount < 1)
                retryCount = 1;

            if (retryDelaySeconds < 1)
                retryDelaySeconds = 1;

            lstResults = new List<List<string>>();

            while (retryCount > 0)
            {
                try
                {
                    using (var dbConnection = new SqlConnection(ConnectStr))
                    {
                        dbConnection.InfoMessage += OnInfoMessage;

                        using (var cmd = new SqlCommand(sqlQuery, dbConnection))
                        {

                            cmd.CommandTimeout = TimeoutSeconds;

                            dbConnection.Open();

                            var reader = cmd.ExecuteReader();

                            while (reader.Read())
                            {
                                var lstCurrentRow = new List<string>();

                                for (var columnIndex = 0; columnIndex <= reader.FieldCount - 1; columnIndex++)
                                {
                                    var value = reader.GetValue(columnIndex);

                                    if (DBNull.Value.Equals(value))
                                    {
                                        lstCurrentRow.Add(string.Empty);
                                    }
                                    else
                                    {
                                        lstCurrentRow.Add(value.ToString());
                                    }

                                }

                                lstResults.Add(lstCurrentRow);

                                if (maxRowsToReturn > 0 && lstResults.Count >= maxRowsToReturn)
                                {
                                    break;
                                }
                            }

                        }
                    }

                    return true;

                }
                catch (Exception ex)
                {
                    retryCount -= 1;
                    if (string.IsNullOrWhiteSpace(callingFunction))
                    {
                        callingFunction = "Unknown";
                    }
                    var errorMessage = string.Format("Exception querying database (called from {0}): {1}; " + "ConnectionString: {2}, RetryCount = {3}, Query {4}", callingFunction, ex.Message, ConnectStr, retryCount, sqlQuery);

                    OnErrorEvent(errorMessage);

                    if (ex.Message.IndexOf("Login failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ex.Message.IndexOf("Invalid object name", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ex.Message.IndexOf("Invalid column name", StringComparison.OrdinalIgnoreCase) >= 0 ||
                        ex.Message.IndexOf("permission was denied", StringComparison.OrdinalIgnoreCase) >= 0)
                    {
                        // No point in retrying the query; it will fail again
                        return false;
                    }

                    // Delay for 5 seconds before trying again
                    clsProgRunner.SleepMilliseconds(retryDelaySeconds * 1000);

                }
            }

            return false;

        }

        /// <summary>
        /// The function updates a database table as specified in the SQL statement.
        /// </summary>
        /// <param name="SQL">A SQL string.</param>
        /// <param name="affectedRows">Affected Rows to be updated.</param>
        /// <return>Returns Boolean showing if the database was updated.</return>
        [Obsolete("Functionality of this method has been disabled for safety; an exception will be raised if it is called")]
        public bool UpdateDatabase(string SQL, out int affectedRows)
        {
            affectedRows = 0;

            throw new Exception("This method is obsolete (because it blindly executes the SQL); do not use");

            /*
                // Updates a database table as specified in the SQL statement

                affectedRows = 0;

                // Verify database connection is open
                if (!OpenConnection())
                    return false;

                try
                {
                    var cmd = new SqlCommand(SQL, m_DBCn);
                    affectedRows = cmd.ExecuteNonQuery();
                    return true;
                }
                catch (Exception ex)
                {
                    // If error happened, log it
                    OnError("Error updating database", ex);
                    return false;
                }
                finally
                {
                    m_DBCn.Close();
                }

              */

        }
    }
}
