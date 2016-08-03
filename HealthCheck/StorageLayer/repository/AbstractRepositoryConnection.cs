using System;
using System.Data;

namespace StorageLayer.repository
{
    public class AbstractRepositoryConnection : IDisposable
    {
        //private static Logger log = new Logger(typeof(LucyRepository));

        public string Repository { get; private set; }
        private IDbConnection _connection;

        /// <summary>
        /// Constructor intended for unit tests only
        /// </summary>
        public AbstractRepositoryConnection()
        {
        }

        public AbstractRepositoryConnection(string repository)
        {
            this.Repository = repository;

            var connectionString = StorageAccounts.GetConnectionString(this.Repository, throwExceptionIfUndefined: true);
        }

        public virtual IDbConnection GetOrCreateConnectionWithRetry()
        {
            if (this._connection == null)
            {
                //log.AddErrorContext("Repository", this.Repository);
                var profiledDbConnection = ConnectionUtil.GetConnectionWithRetry(this.Repository);     //<- This will retry the db connection, if an error occurs

                this._connection = profiledDbConnection;
            }
            return this._connection;
        }



        public void Dispose()
        {
            Dispose(true);
        }

        protected virtual void Dispose(bool disposeAll)
        {
            Close();
        }

        public void Close()
        {
            try
            {
                if ((this._connection != null) && (this._connection.State != System.Data.ConnectionState.Closed))
                {
                    try
                    {
                        this._connection.Close();
                    }
                    catch (Exception ex)
                    {
                        //log.Warning("Closing connection: " + Strings.FullErrorMessage(ex));
                    }
                }
            }
            finally
            {
                this._connection = null;
            }
        }

    }

}

