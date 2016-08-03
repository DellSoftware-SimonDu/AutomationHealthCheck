using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading;
using Utils;

namespace StorageLayer.repository
{
    public abstract class AbstractRepository<T> : IDisposable where T : AbstractRepositoryEntity, new()
    {
        protected AbstractRepositoryConnection AbstractRepositoryConnection { get; set; }

        private bool AreWeRunningAUnitTest { get; set; }

        protected AbstractRepository(AbstractRepositoryConnection connection)
        {
            AbstractRepositoryConnection = connection;
        }

        protected AbstractRepository(string repository)
            : this(new AbstractRepositoryConnection(repository))
        {
        }

        protected IDbConnection GetResolvedDbConnection()
        {
            var result = this.AbstractRepositoryConnection.GetOrCreateConnectionWithRetry();
            return result;
        }

        public virtual T Add(T item)
        {
            throw new NotImplementedException();
        }

        public virtual void Delete(T item)
        {
            throw new NotImplementedException();
        }

        public virtual List<T> Select()
        {
            throw new NotImplementedException();
        }

        public abstract string GetTableName();

        protected IDbCommand GetDefaultCommand(DbTransaction transaction = null)
        {
            var connection = this.GetResolvedDbConnection();
            var command = connection.CreateCommand();
            if (transaction != null)
                command.Transaction = transaction;
            return command;
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposeAll)
        {
            Close();
        }

        protected void Close()
        {
            try
            {
                if (this.AbstractRepositoryConnection != null)
                    this.AbstractRepositoryConnection.Close();

                // NB: this.AbstractRepositoryConnection should remain instantiated, as we might (re)open it.
            }
            catch (Exception ex)
            {
                UnitTestMsg("Error closing database connection: " + Strings.FullErrorMessage(ex));
                // gulp;
            }
        }

        protected long ExecuteAutoIncrementingInsertWithRetries(IDbCommand cmd)
        {
            cmd.CommandText += "; select @@identity";
            long result = ExecuteSingletonSelectWithRetries<ScalarDecimalEntity>(cmd).Value;
            if (result == 0)
            {
                throw new Exception("Autoincrementing insert returned no rows.");
            }
            return result;
        }

        public int ExecuteNonQueryWithRetries(IDbCommand dbCommand, Func<Exception, bool> ignoreExceptionPredicate = null)
        {
            ExecuteDbCommandWithRetriesDelegate<int> executeDbCommandWithRetriesDelegate = delegate (IDbCommand cmd, int attempt)
            {
                try
                {
                    var rowsAffected = cmd.ExecuteNonQuery();
                    return rowsAffected;
                }
                catch (Exception ex)
                {
                    if (ignoreExceptionPredicate != null && ignoreExceptionPredicate(ex))
                    {
                        //gulp
                        return 0;
                    }
                    throw;
                }
            };

            var result = ExecuteDbCommandWithRetries<int>(dbCommand, executeDbCommandWithRetriesDelegate, "ExecuteNonQueryWithRetries");
            return result;
        }

        protected T ExecuteSingletonSelectWithRetries(IDbCommand cmd)
        {
            return ExecuteSingletonSelectWithRetries<T>(cmd);
        }

        protected TEntityType ExecuteSingletonSelectWithRetries<TEntityType>(IDbCommand dbCommand)
            where TEntityType : AbstractRepositoryEntity, new()
        {

            ExecuteDbCommandWithRetriesDelegate<TEntityType> executeDbCommandWithRetriesDelegate = delegate (IDbCommand cmd, int attempt)
            {
                var rdr = cmd.ExecuteReader();
                try
                {
                    var reader = rdr as DbDataReader;
                    if (reader != null && !reader.HasRows)
                        return null;

                    while (rdr.Read())
                    {
                        TEntityType p = new TEntityType();
                        p.InitializeFromRow(rdr);
                        return p;
                    }
                    return null;
                }
                finally
                {
                    rdr.Close();
                }
            };

            var result = ExecuteDbCommandWithRetries<TEntityType>(dbCommand, executeDbCommandWithRetriesDelegate, "ExecuteSingletonSelectWithRetries");
            return result;
        }

        protected List<T> ExecuteSelectWithRetries(IDbCommand cmd)
        {
            return ExecuteSelectWithRetries<T>(cmd);
        }

        protected List<TEntityType> ExecuteSelectWithRetries<TEntityType>(IDbCommand dbCommand)
            where TEntityType : AbstractRepositoryEntity, new()
        {

            ExecuteDbCommandWithRetriesDelegate<List<TEntityType>> executeDbCommandWithRetriesDelegate = delegate (IDbCommand cmd, int attempt)
            {
                var results = new List<TEntityType>();

                var rdr = cmd.ExecuteReader();
                try
                {
                    while (rdr.Read())
                    {
                        TEntityType p = new TEntityType();
                        p.InitializeFromRow(rdr);
                        results.Add(p);
                    }
                    return results;
                }
                finally
                {
                    rdr.Close();
                }
            };

            var result = ExecuteDbCommandWithRetries<List<TEntityType>>(dbCommand, executeDbCommandWithRetriesDelegate, "ExecuteSelectWithRetries");
            return result;
        }

        protected Int32 ConnectionRetryWaitSeconds(Int32 attempt)
        {
            Int32 connectionRetryWaitMilliSeconds = 5 * 1000;
            connectionRetryWaitMilliSeconds = connectionRetryWaitMilliSeconds * (Int32)Math.Pow(2, attempt);

            // If this is a unit-test, shorten the wait...
            if (this.AreWeRunningAUnitTest)
                return attempt * 1000;

            return connectionRetryWaitMilliSeconds;
        }

        /// <summary>
        /// Determine from the exception if the execution
        /// of the connection should Be attempted again
        /// </summary>
        /// <param name="exception">Generic Exception</param>
        /// <returns>True if a a retry is needed, false if not</returns>
        protected static Boolean RetryLitmus(SqlException sqlException)
        {
            switch (sqlException.Number)
            {

                case -2:
                // Timeout expired.  The timeout period elapsed prior to 
                // completion of the operation or the server is not responding.
                // The statement has been terminated.

                case 0:
                // The connection is broken and recovery is not possible. 
                // The connection is marked by the server as unrecoverable. 
                // No attempt was made to restore the connection.

                case 64:
                // A connection was successfully established with the server, 
                // but then an error occurred during the login process.
                // The specified network name is no longer available.

                case 121:
                // A transport-level error has occurred when receiving 
                // results from the server. 
                // The semaphore timeout period has expired.   

                case 233:
                // The client was unable to establish a connection because of an error during 
                // connection initialization process before login.
                // Possible causes include the following: the client tried to connect to an unsupported 
                // version of SQL Server; the server was too busy to accept new connections; or there was 
                // a resource limitation (insufficient memory or maximum allowed connections) on the server.
                // An existing connection was forcibly closed by the remote host.

                case 1205:
                // Transaction (Process ID <N>) was deadlocked on lock resources 
                // with another process and has been chosen as the deadlock victim. 
                // Rerun the transaction.

                case 40143:
                // The service has encountered an error processing your request. Please try again.

                case 40197:
                // The service has encountered an error processing your request. Please try again.

                case 40501:
                // The service is currently busy. Retry the request after 10 seconds.

                case 40613:
                // Database XXXX on server YYYY is not currently available. 
                // Please retry the connection later. If the problem persists, contact customer
                // support, and provide them the session tracing ID of ZZZZZ.

                case 41301:
                //A previous transaction that the current transaction took a dependency on has 
                //aborted, and the current transaction can no longer commit.

                //case 41302:
                //The current transaction attempted to update a record that has been updated since this 
                //transaction started. The transaction was aborted.

                case 41305:
                //The current transaction failed to commit due to a repeatable read validation failure.

                case 41325:
                //The current transaction failed to commit due to a serializable validation failure.

                case 10053:
                // A transport-level error has occurred when receiving results from the server. 
                // An established connection was aborted by the software in your host machine.

                case 10054:
                // A transport-level error has occurred when receiving results from the server.
                // An existing connection was forcibly closed by the remote host.

                case 10060:
                    // A network-related or instance-specific error occurred while establishing a connection to SQL Server.
                    // The server was not found or was not accessible. Verify that the instance name is correct and that SQL Server
                    // is configured to allow remote connections. 
                    // A connection attempt failed because the connected party did not properly respond after a period of time, 
                    // or established connection failed because connected host has failed to respond.


                    return (true);

            }
            if ((sqlException.Number != 219) && //The type 'blah' already exists, or you do not have permission to create it.
                    (sqlException.Number != 515) && //Cannot insert the value NULL into column 'column', table 'table'; column does not allow nulls. INSERT fails. The statement has been terminated.
                    (sqlException.Number != 2627) && //Cannot insert duplicate key in object 'object'. The statement has been terminated.
                    (sqlException.Number != 2601)) //Cannot insert duplicate key row in object 'object' with unique index 'index'. The statement has been terminated.
            {
                //log.Warning("RetryLitmus: Falling thru...Error text [{0}], Number [{1}]", sqlException.Message, sqlException.Number);
            }
            return (false);
        }

        protected virtual int GetBatchInsertSize()
        {
            return 5000;
        }

        //public Encryption Encryption
        //{
        //    get
        //    {
        //        if (this.encryption == null)
        //            this.encryption = new Encryption();

        //        return this.encryption;
        //    }
        //}

        public List<TAdhocEntity> AdhocSelectWithRetries<TAdhocEntity>(string sql)
            where TAdhocEntity : AbstractRepositoryEntity, new()
        {
            var cmd = GetDefaultCommand();
            cmd.CommandText = sql;
            return ExecuteSelectWithRetries<TAdhocEntity>(cmd);
        }

        public List<T> AdhocSelectWithRetries(string sql)
        {
            var cmd = GetDefaultCommand();
            cmd.CommandText = sql;
            return ExecuteSelectWithRetries(cmd);
        }

        public int AdhocNonQueryWithRetries(string sql)
        {
            var cmd = GetDefaultCommand();
            cmd.CommandText = sql;
            return ExecuteNonQueryWithRetries(cmd);
        }

        public int AdhocNonQueryWithRetries(string sql, TimeSpan timeout)
        {
            var cmd = GetDefaultCommand();
            cmd.CommandTimeout = (int)Math.Round(timeout.TotalSeconds);
            cmd.CommandText = sql;
            return ExecuteNonQueryWithRetries(cmd);
        }

        private string _db_name = null;
        public string db_name()
        {
            if (this._db_name == null)
            {
                var cmd = GetDefaultCommand();
                cmd.CommandText = "SELECT db_name()";
                this._db_name = ExecuteSingletonSelectWithRetries<ScalarStringEntity>(cmd).Value;
            }

            return this._db_name;
        }

        //protected void LogInsert(T item, long ID)
        //{
        //    var json = JsonConvert.SerializeObject(item, new Newtonsoft.Json.Converters.StringEnumConverter());

        //    using (var backupDimensionIDContext = BackupDimensionIDsContext.GetInstance())
        //    {
        //        try
        //        {
        //            backupDimensionIDContext.Add(new BackupDimensionIDsEntity
        //            {
        //                DatabaseName = this.db_name(),
        //                TableName = GetTableName(),
        //                EntityClass = item.GetType().FullName,
        //                ID = ID,
        //                EntityJson = json,
        //                PartitionKey = BackupDimensionIDsEntity.CreatePartitionKey(this.db_name(), GetTableName()),
        //                RowKey = BackupDimensionIDsEntity.CreateRowKey(ID),
        //                Timestamp = DateTime.UtcNow,
        //            });
        //        }
        //        catch (DataServiceRequestException ex)
        //        {
        //            var innerException = ex.InnerException as DataServiceClientException;
        //            if (innerException == null || innerException.StatusCode != 409) // Gulp: ignore "The specified entity already exists."
        //            {
        //                throw;
        //            }
        //        }
        //    }
        //}

        public IDbTransaction BeginTransaction()
        {
            return this.GetResolvedDbConnection().BeginTransaction();
        }

        public void Commit(DbTransaction transaction)
        {
            transaction.Commit();
        }

        //protected void CacheDimID(object value, params object[] keyFragments)
        //{
        //    var key = GetTableName() + string.Join(";", keyFragments);
        //    LucyCache.Instance.Put(key, value);
        //}

        //protected TId? GetCachedDimID<TId>(params object[] keyFragments)
        //    where TId : struct
        //{
        //    var key = GetTableName() + string.Join(";", keyFragments);
        //    TId result;
        //    return LucyCache.Instance.Get(key, out result) == RedisCache.CacheResult.Hit
        //        ? result : (TId?)null;
        //}

        //protected T GetCachedDimObject<T>(params object[] keyFragments)
        //{
        //    var key = GetTableName() + string.Join(";", keyFragments);
        //    T result;
        //    LucyCache.Instance.Get(key, out result);
        //    return result;
        //}

        private void UnitTestMsg(string msg, params object[] frags)
        {
            if (this.AreWeRunningAUnitTest)
                Console.WriteLine(msg, frags);
        }

        //protected void UnitTestExecuteDbCommandWithRetries()
        //{
        //    var areWeRunningAUnitTest = this.AreWeRunningAUnitTest;
        //    try
        //    {
        //        this.AreWeRunningAUnitTest = true;

        //        //==========================================================================
        //        //                    Select a Single Value from a table
        //        //==========================================================================           
        //        {
        //            ExecuteDbCommandWithRetriesDelegate<CommonEntities.StringEntity> executeDbCommandWithRetriesDelegate = delegate (IDbCommand cmd, int attempt)
        //            {
        //                CommonEntities.StringEntity data = null;
        //                using (var rdr = cmd.ExecuteReader())
        //                {
        //                    if (rdr.Read())
        //                    {
        //                        data = new CommonEntities.StringEntity();
        //                        data.InitializeFromRow(rdr);
        //                        return data;
        //                    }
        //                }
        //                return null;
        //            };

        //            var dbCommand = GetDefaultCommand();
        //            dbCommand.CommandText = "select db_name() as value;";

        //            var result = ExecuteDbCommandWithRetries<CommonEntities.StringEntity>(dbCommand, executeDbCommandWithRetriesDelegate, "UNIT TEST");
        //            UnitTestMsg("Result of the query execution: " + result.Value);
        //        }

        //    }
        //    finally
        //    {
        //        this.AreWeRunningAUnitTest = areWeRunningAUnitTest;
        //    }
        //}

        delegate TResult ExecuteDbCommandWithRetriesDelegate<TResult>(IDbCommand cmd, int attempt);

        private const string UNIT_TEST_SQL_PREFIX = "UNIT-TEST : FAIL THIS SQL => ";
        private TResult ExecuteDbCommandWithRetries<TResult>(IDbCommand cmd,
                                                             ExecuteDbCommandWithRetriesDelegate<TResult> executeDbCommandWithRetriesDelegate,
                                                             string delegateDescription)
        {
            UnitTestMsg("======== ExecuteDbCommandWithRetries ========");

            for (Int32 attempt = 1; ;)
            {
                try
                {
                    if (this.AreWeRunningAUnitTest)
                    {
                        if (attempt == 1)
                            cmd.CommandText = UNIT_TEST_SQL_PREFIX + cmd.CommandText;
                        else
                            cmd.CommandText = cmd.CommandText.Replace(UNIT_TEST_SQL_PREFIX, "");
                    }

                    UnitTestMsg(string.Format("{0}[{1}] Attempt {2} of : \"{3}\""
                                        , attempt > 1 ? "\n" : ""
                                        , delegateDescription
                                        , attempt
                                        , cmd.CommandText));

                    var result = executeDbCommandWithRetriesDelegate(cmd, attempt);

                    if (attempt > 1)
                        UnitTestMsg("\t[{0}] Succeeded after {1} tries", delegateDescription, attempt);

                    return result;

                }
                catch (Exception exception)
                {
                    UnitTestMsg("\t[{0}] {1}", exception.GetType().Name, exception.Message);

                    exception.Data["Number of attempts executing this DbCommand"] = attempt;

                    var retryExecutingDbCommand = false;

                    #region Are we unit-testing this code? (and want the retry logic to execute)...
                    {
                        if (this.AreWeRunningAUnitTest)
                        {
                            UnitTestMsg("\t[{0}] We will retry execution of this DbCommand, as this is a unit-test", delegateDescription);
                            retryExecutingDbCommand = true;
                        }
                    }
                    #endregion

                    #region Is the exception a 'SqlException' that can be retried?...
                    {
                        var sqlException = ExceptionUtils.FindInnerSqlException(exception);
                        if (sqlException != null)
                        {
                            UnitTestMsg("\t\t SqlException: ErrorCode={0}, Number={1}"
                                            , sqlException.ErrorCode
                                            , sqlException.Number);

                            exception.Data["sqlException.Number"] = sqlException.Number;
                            exception.Data["sqlException.ErrorCode"] = sqlException.ErrorCode;

                            if (RetryLitmus(sqlException))
                            {
                                UnitTestMsg("\t\t\tRetryLitmus() has indicated that this SQL should be retried.");
                                retryExecutingDbCommand = true;
                            }
                        }
                    }
                    #endregion

                    #region Is the exception an 'InvalidOperationException' that can be retried?...
                    {
                        if (exception is InvalidOperationException)
                        {
                            if (exception.Message.StartsWith("ExecuteReader requires an open and available Connection") ||
                                exception.Message.StartsWith("ExecuteNonQuery requires an open and available Connection"))
                                retryExecutingDbCommand = true;
                        }
                    }
                    #endregion

                    // If the error indicates that we should not retry, then we're done...
                    if (!retryExecutingDbCommand)
                        throw;

                    #region Have we exceeded the 'retry count'?...
                    {
                        // Increment Trys
                        attempt++;

                        // Find Maximum Trys
                        Int32 maxRetryCount = 4;

                        // Throw Error if we have reach the maximum number of retries
                        if (attempt == maxRetryCount)
                        {
                            UnitTestMsg("\t[0] Exceeded retry count", delegateDescription);
                            throw;
                        }
                    }
                    #endregion

                    #region Sleep for a while, before retrying...
                    {
                        var sleepMs = ConnectionRetryWaitSeconds(attempt);
                        UnitTestMsg("\t[{0}] Sleeping for {1} ms, prior to attempt {2}.", delegateDescription, sleepMs, attempt);
                        Thread.Sleep(sleepMs);
                    }
                    #endregion

                    #region Reconnect to the database, (before we retry)...
                    {
                        Close();

                        try
                        {
                            cmd.Connection = this.AbstractRepositoryConnection.GetOrCreateConnectionWithRetry();
                        }
                        catch (Exception lEx)
                        {
                            var msg = string.Format("\t[0] Giving up - could not (re)connect to the database."
                                                            + "\nLast (re)connection attempt error:\n{1}"
                                                            , delegateDescription
                                                            , lEx.Message);

                            UnitTestMsg(msg);

                            exception.Data["context"] = msg;

                            // (re)throw the original exception (about the SQL statement that failed), not the exception from when we tried 
                            // to (re)connect to the database...
                            throw exception;
                        }

                        if (this.AreWeRunningAUnitTest)
                        {
                            var dbNameCommand = cmd.Connection.CreateCommand();
                            dbNameCommand.CommandText = "select db_name()";
                            var name = (string)dbNameCommand.ExecuteScalar();
                            UnitTestMsg("\t[{0}] The database name is \"{1}\".", delegateDescription, name);
                        }

                        UnitTestMsg("\t[{0}] We have successfully (re)connected to the database (so the DbCommand can now be re-attempted)", delegateDescription);
                    }
                    #endregion
                }
            }
        }

        public delegate void DeleteBatchCompleted(long rowsDeletedInBatch, TimeSpan timeToDeleteBatch);

        /// <summary>
        /// Deletes (potentially) a lot of objects from the repository
        /// </summary>
        /// <param name="columnMatches">Filter for deletion - Column names mapped to values</param>
        /// <param name="batchSize">Max number of objects to delete per batch</param>
        /// <param name="afterBatch">Run after each batch. Passed the number of rows that got deleted by the batch.
        /// This will be called once with '0' before the deletion completes.</param>
        /// <param name="transaction">Transaction for the Delete statements</param>
        /// <returns>Total number of rows deleted</returns>
        protected long Delete(Dictionary<string, object> columnMatches,
            int batchSize = 10000, DeleteBatchCompleted afterBatch = null, DbTransaction transaction = null)
        {
            if (columnMatches.Count == 0)
                throw new ArgumentException("columnMatches is required");

            var cmd = GetDefaultCommand();

            if (transaction != null)
                cmd.Transaction = transaction;

            #region SQL DELETE statement and its parameters

            var sql = new StringBuilder();
            sql.AppendLine(string.Format(
                    @"delete top({0}) from {1} where", batchSize, GetTableName()));

            var colEnum = columnMatches.Keys.GetEnumerator();
            colEnum.MoveNext(); // At least one condition must be present
            while (true)
            {
                var column = colEnum.Current;
                sql.Append("  ").Append(column).Append(" = @").Append(column);

                if (colEnum.MoveNext())
                    sql.AppendLine(" and");
                else
                    break;
            }
            sql.AppendLine(";");
            cmd.CommandText = sql.ToString();

            foreach (var columnMatch in columnMatches)
            {
                cmd.Parameters.Add(new SqlParameter('@' + columnMatch.Key, columnMatch.Value));
            }

            #endregion

            long result = 0;
            int rowsProcessed;
            do
            {
                var start = DateTime.UtcNow;
                rowsProcessed = ExecuteNonQueryWithRetries(cmd);

                var elapsed = DateTime.UtcNow - start;
                result += rowsProcessed;

                if (afterBatch != null)
                {
                    afterBatch.Invoke(rowsProcessed, elapsed);
                }
            } while (rowsProcessed > 0);

            return result;
        }

        public BatchProcessingStatistics ExecuteNonQueryInBatches(string sql, Dictionary<string, object> parameters, TimeSpan? interBatchSleepTimeSpan = null, ExecuteNonQueryBatchCompleted interBatchCallback = null)
        {
            var batchProcessingStatistics = new BatchProcessingStatistics();

            var cmd = GetDefaultCommand();
            cmd.CommandText = sql;
            cmd.CommandTimeout = 0;

            if (parameters != null)
            {
                foreach (var parameter in parameters)
                {
                    cmd.Parameters.Add(new SqlParameter(parameter.Key, parameter.Value));
                }
            }

            int? rowsProcessed = null;
            while (!rowsProcessed.HasValue || rowsProcessed.Value > 0)
            {
                var batchStartTime = DateTime.Now;
                rowsProcessed = ExecuteNonQueryWithRetries(cmd);

                batchProcessingStatistics.RowsAffectedByCurrentBatch = rowsProcessed.Value;
                batchProcessingStatistics.TotalRowsAffected += rowsProcessed.Value;
                batchProcessingStatistics.NumberOfBatchesProcessed += 1;
                batchProcessingStatistics.TimeSpentExecutingQueries = batchProcessingStatistics.TimeSpentExecutingQueries.Add(DateTime.Now.Subtract(batchStartTime));

                if (interBatchCallback != null)
                {
                    interBatchCallback(batchProcessingStatistics, ref interBatchSleepTimeSpan);
                }

                if (interBatchSleepTimeSpan.HasValue)
                {
                    Thread.Sleep(interBatchSleepTimeSpan.Value);
                    batchProcessingStatistics.TimeSpentSleepingBetweenBatches = batchProcessingStatistics.TimeSpentSleepingBetweenBatches.Add(interBatchSleepTimeSpan.Value);
                }
            }

            return batchProcessingStatistics;
        }
    }

    public class BatchProcessingStatistics
    {
        public long RowsAffectedByCurrentBatch { get; internal set; }
        public long TotalRowsAffected { get; internal set; }
        public int NumberOfBatchesProcessed { get; internal set; }
        public TimeSpan TimeSpentExecutingQueries = new TimeSpan();
        public TimeSpan TimeSpentSleepingBetweenBatches = new TimeSpan();

        public TimeSpan TotalElapsedTime
        {
            get
            {
                return this.TimeSpentExecutingQueries.Add(this.TimeSpentSleepingBetweenBatches);
            }
        }
    }

    public delegate void ExecuteNonQueryBatchCompleted(BatchProcessingStatistics batchProcessingStatistics, ref TimeSpan? interBatchSleepTimeSpan);

}

