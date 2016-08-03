using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using StorageLayer.repository;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

namespace StorageLayer
{
    public class LucySqlConnection : DbConnection, IDisposable
    {
        private string FullConnectionString { get; set; }
        private SqlConnection _sqlConnection = null;

        public LucySqlConnection(string connectionString)
        {
            this.FullConnectionString = connectionString;

            var sqlConnectionStringBuilder = new SqlConnectionStringBuilder();
            sqlConnectionStringBuilder.ConnectionString = connectionString;

            sqlConnectionStringBuilder.DataSource = string.Empty;
            sqlConnectionStringBuilder.InitialCatalog = string.Empty;
        }

        void IDisposable.Dispose()
        {
            DisposeOfSqlConnection();
        }

        private void DisposeOfSqlConnection()
        {
            if (this._sqlConnection != null)
                this._sqlConnection.Dispose();

            this._sqlConnection = null;
        }

        public SqlConnection CurrentSqlConnection
        {
            get
            {
                return this._sqlConnection;
            }
        }

        public void OpenWithRetry(RetryPolicy retryPolicy)
        {
            OpenDatabaseConnection(retryPolicy);
        }

        public override void Open()
        {
            OpenDatabaseConnection(null);
        }

        private void OpenDatabaseConnection(RetryPolicy retryPolicy)
        {
            if (this._sqlConnection == null)
                this._sqlConnection = new SqlConnection(this.FullConnectionString);

            if (retryPolicy == null)
                this._sqlConnection.Open();
            else
                this._sqlConnection.OpenWithRetry(retryPolicy);
        }

        private void EnsureSqlConnectionCurrentlyExists()
        {
            if (this._sqlConnection == null)
                throw new ApplicationException("No SqlConnection currently exists.");
        }

        protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel)
        {
            EnsureSqlConnectionCurrentlyExists();

            return this._sqlConnection.BeginTransaction(isolationLevel);
        }

        public override void ChangeDatabase(string databaseName)
        {
            EnsureSqlConnectionCurrentlyExists();

            this._sqlConnection.ChangeDatabase(databaseName);
        }

        public override void Close()
        {
            if (this._sqlConnection != null)
                this._sqlConnection.Close();
        }

        public override string ConnectionString
        {
            get
            {
                return this.FullConnectionString;
            }
            set
            {
                throw new NotSupportedException("Altering the SQL Server connection string is not supported."
                                                + "\nInstead, discard this connection and create another.");
            }
        }

        protected override DbCommand CreateDbCommand()
        {
            var command = new LucySqlCommand(this);
            command.CommandTimeout = 1800;  //<- 1800 seconds = 30 minutes
            return command;

            /*
             * Don't set the CommandTimeout to zero (to disable timeouts) as this will prevent
             * the ability of the query to timeout and be retried. If timeouts are disabled, then
             * if a query 'gets stuck', the worker-role for example, will become 'stuck' too and
             * will have to be restarted in order for the query to be re-attempted.
             */
        }

        public override string Database
        {
            get
            {
                EnsureSqlConnectionCurrentlyExists();
                return this._sqlConnection.Database;
            }
        }

        public override string DataSource
        {
            get
            {
                EnsureSqlConnectionCurrentlyExists();
                return this._sqlConnection.DataSource;
            }
        }

        public override string ServerVersion
        {
            get
            {
                EnsureSqlConnectionCurrentlyExists();
                return this._sqlConnection.ServerVersion;
            }
        }

        public override ConnectionState State
        {
            get
            {
                EnsureSqlConnectionCurrentlyExists();
                return this._sqlConnection.State;
            }
        }

        public static LucySqlConnection FromDbConnection(IDbConnection dbConnection)
        {
            if (dbConnection == null)
                return null;

            if (dbConnection is LucySqlConnection)
                return (dbConnection as LucySqlConnection);

            if (dbConnection is MvcMiniProfiler.Data.ProfiledDbConnection)
            {
                var wrappedConnection = ((MvcMiniProfiler.Data.ProfiledDbConnection)dbConnection).WrappedConnection;
                return LucySqlConnection.FromDbConnection(wrappedConnection);
            }

            return null;
        }
    }
}
