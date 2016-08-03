using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MvcMiniProfiler.Data;
using Microsoft.Practices.EnterpriseLibrary.TransientFaultHandling;
using System.Data.SqlClient;

namespace StorageLayer
{
    class ConnectionUtil
    {

        //private static Logger log = new Logger(typeof(LucyRepository));

        public static string GetConnectionStringValue(string name, bool throwExceptionIfUndefined = false)
        {
            return StorageAccounts.GetConnectionString(name, throwExceptionIfUndefined);
        }

        public static ProfiledDbConnection GetConnectionWithRetry(string connectionStringName)
        {
            try
            {
                string connStr = GetConnectionStringValue(connectionStringName, throwExceptionIfUndefined: true);

                return GetProfiledDbConnectionWithRetry(connStr);
            }
            catch (Exception ex)
            {
                ex.Data["connectionStringName"] = connectionStringName;
                throw;
            }
        }

        public static ProfiledDbConnection GetProfiledDbConnectionWithRetry(string connStr)
        {
            var retryStrategy = new ExponentialBackoff(20
                                                        , TimeSpan.FromSeconds(1)
                                                        , TimeSpan.FromSeconds(30)
                                                        , TimeSpan.FromSeconds(1)
                                                        );

            var retryPolicy = new RetryPolicy<SqlDatabaseTransientErrorDetectionStrategy>(retryStrategy);

            //retryPolicy.Retrying += (sender, args) => log.Warning("Failed to connect to SQL Azure, retrying."
            //                                                        + "\r\n\r\nRetry count: {0}, delay: {1}, exception: {2}"
            //                                                        , args.CurrentRetryCount
            //                                                        , args.Delay
            //                                                        , args.LastException
            //                                                        );

            var conn = new LucySqlConnection(connStr);
            try
            {
                conn.OpenWithRetry(retryPolicy);
            }
            catch (Exception e)
            {
                //var sqlException = e as SqlException;
                //if (null != sqlException)
                //{
                //    foreach (SqlError err in sqlException.Errors)
                //    {
                //        string databaseName = GetDBNameFromConnectionString(connStr);

                //        switch (err.Number)
                //        {
                //            case (-2):
                //                {
                //                    var azureTimeoutDetails = new AzureTimeoutDetails
                //                    {
                //                        Timestamp = DateTime.UtcNow,
                //                        DatabaseName = databaseName,
                //                        Message = e.Message,
                //                        StackTrace = e.StackTrace,
                //                        Tags = string.Format("{0}|{1}|{2}"
                //                                                , LucyConstants.SqlServer.DatabaseErrors.WadLogMessageCategory.DbConnectionTimeout
                //                                                , null
                //                                                , "AzureTimeoutDetails")
                //                    };

                //                    //log.Error(JsonConvert.SerializeObject(azureTimeoutDetails, new Newtonsoft.Json.Converters.StringEnumConverter()));
                //                    break;
                //                }
                //            // SQL Error Code: 40613
                //            // Database XXXX on server YYYY is not currently available. Please retry the connection later.
                //            // If the problem persists, contact customer support, and provide them the session tracing ID of ZZZZZ.
                //            case (40613):
                //               // log.Error(e);
                //                break;
                //            default:
                //                break;
                //        }
                //    }
                //}
                throw;
            }

            var result = new ProfiledDbConnection(conn, MvcMiniProfiler.MiniProfiler.Current);
            return result;
        }

    }
}
