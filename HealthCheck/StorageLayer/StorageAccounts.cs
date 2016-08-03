using Microsoft.WindowsAzure.Storage;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLayer
{
    class StorageAccounts
    {
        public readonly static StorageAccounts Instance = new StorageAccounts();

        public const string DataConnectionStringName = "DataConnectionString";
        public const string TestDataConnectionStringName = "Test-DataConnectionString";
        public const string LogConnectionStringName = "LogConnectionString";
        public const string ApiLogConnectionStringName = "ApiLogConnectionString";
        public const string HadoopDataConnectionStringName = "HadoopDataConnectionString";
        public const string TestHadoopDataConnectionStringName = "Test-HadoopDataConnectionString";
        public const string SpotlightDataConnectionStringName = "SpotlightDataConnectionString";
        public const string BackupConnectionStringName = "BackupConnectionString";
        public const string DiagnosticsConnectionStringName = "DiagnosticsConnectionString";
        public const string CapacityAggregateStorageConnectionStringName = "CapacityAggregateStorageConnectionString";

        public const string LogAnalyticsConnectionStringName = "LogAnalyticsConnectionString";
        public const string ReadDiagnosticsConnectionStringName = "ReadDiagnosticsConnectionString";
        public const string TestReadDiagnosticsConnectionStringName = "Test-ReadDiagnosticsConnectionString";
        public const string TestSpotlightDataConnectionStringName = "Test-SpotlightDataConnectionString";
        public const string TestLogConnectionStringName = "Test-LogConnectionString";
        public const string TestDiagnosticsConnectionStringName = "Test-DiagnosticsConnectionString";

        public static Func<string, string> ConfigValueGetter;

        protected StorageAccounts()
        {
        }

        public static CloudStorageAccount CreateCloudStorageAccountFromNamedConnectionString(string connectionStringName)
        {
            string connectionStringValue = null;
            try
            {
                connectionStringValue = GetConnectionString(connectionStringName);
                return CloudStorageAccount.Parse(connectionStringValue);
            }
            catch (Exception ex)
            {
                ex.Data["connectionStringName"] = connectionStringName;
                ex.Data["connectionStringValue"] = connectionStringValue ?? "<null>";
                throw;
            }
        }

        public static string GetConnectionString(string name, bool throwExceptionIfUndefined = false)
        {
            var result = "";

            if (ConfigValueGetter != null)
            {
                result = ConfigValueGetter(name);
                if (!string.IsNullOrWhiteSpace(result))
                    return result;
            }

            // Read out of a local web.config or app.config file
            if (ConfigurationManager.ConnectionStrings[name] != null)
                result = ConfigurationManager.ConnectionStrings[name].ConnectionString;

            if (string.IsNullOrWhiteSpace(result))
                result = ConfigurationManager.AppSettings[name];

            // try keyvault last
            //if (string.IsNullOrWhiteSpace(result))
            //{
            //    if (!name.Equals("KVClientID", StringComparison.InvariantCultureIgnoreCase) &&
            //       !name.Equals("KVThumbprint", StringComparison.InvariantCultureIgnoreCase)
            //      )
            //    {
            //        var kvClientId = GetConnectionString("KVClientID", throwExceptionIfUndefined: false);
            //        var kvThumbprint = GetConnectionString("KVThumbprint", throwExceptionIfUndefined: false);
            //        if (!string.IsNullOrWhiteSpace(kvClientId) && !string.IsNullOrWhiteSpace(kvThumbprint))
            //            result = KeyVaultUtils.Get(kvClientId, kvThumbprint).GetSecret(ConfigValueGetter, "KV" + name);
            //    }
            //}

            if (throwExceptionIfUndefined && string.IsNullOrWhiteSpace(result))
                throw new ApplicationException(string.Format("Unable to obtain a value for the connect string named \"{0}\"", name));

            return result;
        }

        public virtual CloudStorageAccount Default
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    DataConnectionStringName);
            }
        }

        public virtual CloudStorageAccount Logs
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    LogConnectionStringName);
            }
        }

        public virtual CloudStorageAccount ApiLogs
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    ApiLogConnectionStringName);
            }
        }

        public virtual CloudStorageAccount Hadoop
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    HadoopDataConnectionStringName);
            }
        }

        public virtual CloudStorageAccount Spotlight
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    SpotlightDataConnectionStringName);
            }
        }

        public virtual CloudStorageAccount Backup
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    BackupConnectionStringName);
            }
        }

        public virtual CloudStorageAccount CapacityPlanning
        {
            get
            {
                return CreateCloudStorageAccountFromNamedConnectionString(
                    CapacityAggregateStorageConnectionStringName);
            }
        }

    }
}
