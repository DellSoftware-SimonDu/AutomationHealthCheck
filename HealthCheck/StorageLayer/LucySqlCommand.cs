using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;

namespace StorageLayer
{
    public class LucySqlCommand : DbCommand, IDisposable
    {
        //private static Logger log = new Logger(typeof(LucySqlCommand));

        protected LucySqlConnection LucySqlConnection { get; set; }
        protected SqlCommand SqlCommand { get; set; }

        public LucySqlCommand(LucySqlConnection lucySqlConnection)
        {
            this.LucySqlConnection = lucySqlConnection;
            this.SqlCommand = new SqlCommand("", lucySqlConnection.CurrentSqlConnection);
        }

        void IDisposable.Dispose()
        {
            if (this.SqlCommand != null)
                this.SqlCommand.Dispose();

            this.SqlCommand = null;
        }

        public override void Cancel()
        {
            this.SqlCommand.Cancel();
        }

        public override string CommandText
        {
            get
            {
                return this.SqlCommand.CommandText;
            }
            set
            {
                this.SqlCommand.CommandText = value;
            }
        }

        public override int CommandTimeout
        {
            get
            {
                return this.SqlCommand.CommandTimeout;
            }
            set
            {
                this.SqlCommand.CommandTimeout = value;
            }
        }

        public override CommandType CommandType
        {
            get
            {
                return this.SqlCommand.CommandType;
            }
            set
            {
                this.SqlCommand.CommandType = value;
            }
        }

        protected override DbParameter CreateDbParameter()
        {
            return this.SqlCommand.CreateParameter();
        }

        protected override DbConnection DbConnection
        {
            get
            {
                return this.SqlCommand.Connection;
            }
            set
            {
                this.SqlCommand.Connection = ((LucySqlConnection)value).CurrentSqlConnection;
            }
        }

        protected override DbParameterCollection DbParameterCollection
        {
            get { return this.SqlCommand.Parameters; }
        }

        protected override DbTransaction DbTransaction
        {
            get
            {
                return this.SqlCommand.Transaction;
            }
            set
            {
                this.SqlCommand.Transaction = (SqlTransaction)value;
            }
        }

        public override bool DesignTimeVisible
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        //private LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation GetSqlExceptionExplanation(Exception ex)
        //{
        //    if (ex == null)
        //        return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.unknown;

        //    if (ex is SqlException)
        //    {
        //        switch ((ex as SqlException).Number)
        //        {
        //            case -2: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.clientTimeout;
        //            case 701: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.outOfMemory;
        //            case 1204: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.lockRequest;
        //            case 1205: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.deadlockVictim;
        //            case 1222: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.lockRequestTimeout;
        //            case 8645: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.timeoutWaitingforMemoryResource;
        //            case 8651: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.lowMemoryCondition;
        //            default: return LucyConstants.SqlServer.DatabaseErrors.SqlExceptionExplanation.unknown;
        //        }
        //    }

        //    return GetSqlExceptionExplanation(ex.InnerException);
        //}

        //private void LogQueryExecutionError(Exception ex)
        //{
        //    var messageSubCategory = GetSqlExceptionExplanation(ex);

        //    var azureTimeoutDetails = new AzureTimeoutDetails
        //    {
        //        Timestamp = DateTime.UtcNow,
        //        Message = ex.Message,
        //        StackTrace = ex.StackTrace,
        //        FullStackTrace = Environment.StackTrace,
        //        Tags = string.Format("{0}|{1}|{2}"
        //                                , LucyConstants.SqlServer.DatabaseErrors.WadLogMessageCategory.sqlQueryExecutionError
        //                                , messageSubCategory
        //                                , "AzureTimeoutDetails"
        //                               )
        //    };

        //    log.Error(JsonConvert.SerializeObject(azureTimeoutDetails, new Newtonsoft.Json.Converters.StringEnumConverter()));
        //}

        protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior)
        {
            try
            {
                return this.SqlCommand.ExecuteReader(behavior);
            }
            catch (Exception ex)
            {
                //LogQueryExecutionError(ex);
                throw;
            }
        }

        public override int ExecuteNonQuery()
        {
            return this.SqlCommand.ExecuteNonQuery();
        }

        public override object ExecuteScalar()
        {
            try
            {
                return this.SqlCommand.ExecuteScalar();
            }
            catch (Exception ex)
            {
                //LogQueryExecutionError(ex);
                throw;
            }
        }

        public override void Prepare()
        {
            this.SqlCommand.Prepare();
        }

        public override UpdateRowSource UpdatedRowSource
        {
            get
            {
                return this.SqlCommand.UpdatedRowSource;
            }
            set
            {
                this.SqlCommand.UpdatedRowSource = value;
            }
        }


    }
}
