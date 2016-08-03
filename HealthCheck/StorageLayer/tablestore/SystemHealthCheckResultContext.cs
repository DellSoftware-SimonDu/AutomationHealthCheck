using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Table;
using MoreLinq;

namespace StorageLayer.tablestore
{
    public class SystemHealthCheckResultContext
    {
        private CloudTable Table { get; set; }

        public SystemHealthCheckResultContext()
        {
            var cloudTableClient = StorageAccounts.Instance.Default.CreateCloudTableClient();
            Table = cloudTableClient.GetTableReference("SystemHealthCheckResult");
        }

        public IEnumerable<SystemHealthCheckResultEntity> SelectLatest(Guid ownerGuid)
        {
            var pkEqualTo = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                SystemHealthCheckResultEntity.CreatePartitionKey(ownerGuid));

            var query = new TableQuery<SystemHealthCheckResultEntity>().Where(pkEqualTo);
            var all = query.ExecuteSegmentedOn(Table);

            // Today's healthchecks could be in progress. So read the latest *2* dates available.
            var latestDates = new HashSet<DateTime>();
            var recent = all.TakeWhile(_ =>
            {
                latestDates.Add(_.ObservationDate);
                return latestDates.Count < 3;
            }).ToList();

            // Dedupe for the normal case where we have a healthcheck both days, take the latest.
            var latest = from hc in recent
                         group hc by new { hc.DiagnosticServerId, hc.ConnectionName } into g
                         select g.MaxBy(_ => _.ObservationDate);

            return latest;
        }

        // TODO review what caller wants - it may need to Select over two dates, like SelectLatest now does.
        public IEnumerable<SystemHealthCheckResultEntity> Select(Guid ownerGuid, DateTime observationDateUtc)
        {
            var pkEqualTo = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal,
                SystemHealthCheckResultEntity.CreatePartitionKey(ownerGuid));

            var rkPrefix = SystemHealthCheckResultEntity.CreateRowKeyPrefix(observationDateUtc);
            var rkGreaterThan = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThan, rkPrefix + ':');
            var rkLessThan = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThan, rkPrefix + ';');

            var query = new TableQuery<SystemHealthCheckResultEntity>().Where(TableQuery.CombineFilters(
                        pkEqualTo,
                        TableOperators.And,
                        TableQuery.CombineFilters(
                            rkGreaterThan,
                            TableOperators.And,
                            rkLessThan)));

            var results = query.ExecuteSegmentedOn(Table);
            return results;
        }

        public SystemHealthCheckResultEntity Select(Guid ownerGuid, DateTime observationDateUtc, Guid dsId, string connectionName)
        {
            var retrieve = TableOperation.Retrieve<SystemHealthCheckResultEntity>(
                SystemHealthCheckResultEntity.CreatePartitionKey(ownerGuid),
                SystemHealthCheckResultEntity.CreateRowKey(observationDateUtc, dsId, connectionName)
                );

            var result = Table.Execute(retrieve);
            return (SystemHealthCheckResultEntity)result.Result; // Null if not found
        }


        public void Upsert(SystemHealthCheckResultEntity toSave)
        {
            // Pay this cost on writes only
            // (writes are from healthcheck worker role - batch process, doesn't care)
            Table.CreateIfNotExists();

            Table.Execute(TableOperation.InsertOrReplace(toSave));
        }

    }

    public class SystemHealthCheckResultEntity : TableEntity
    {
        public Guid OwnerGuid { get; set; }
        public Guid DiagnosticServerId { get; set; }
        public string ConnectionName { get; set; }
        public DateTime ObservationDate { get; set; }
        public DateTime PerformedTime { get; set; }
        public int HighPriorityCount { get; set; }
        public int MediumPriorityCount { get; set; }
        public int LowPriorityCount { get; set; }
        public int NumHealthCheckResults { get { return _healthCheckResults.Count; } }

        /// <summary>
        /// Values:
        /// 0-100 score
        /// -1    no result
        /// </summary>
        [IgnoreProperty]
        public IDictionary<string, double> HealthCheckResults
        {
            get { return new ReadOnlyDictionary<string, double>(_healthCheckResults); }
            set
            {
                if (value.Any(kv => kv.Value < -1 || kv.Value > 100))
                    throw new ApplicationException();

                _healthCheckResults = CopyDictionary(value);
            }
        }
        private IDictionary<string, double> _healthCheckResults;


        public SystemHealthCheckResultEntity()
        {
        }

        public SystemHealthCheckResultEntity(DateTime observationDateUtc, Guid ownerGuid, Guid diagnosticServerId, string connectionName)
            : this()
        {
            OwnerGuid = ownerGuid;
            DiagnosticServerId = diagnosticServerId;
            ConnectionName = connectionName;
            ObservationDate = observationDateUtc.Date;

            PartitionKey = CreatePartitionKey(ownerGuid);
            RowKey = CreateRowKey(observationDateUtc.Date, diagnosticServerId, connectionName);
        }

        public static string CreatePartitionKey(Guid ownerGuid)
        {
            return ownerGuid.ToString().ToUpperInvariant();
        }

        public static string CreateRowKey(DateTime observationDateUtc, Guid diagnosticServerId, string connectionName)
        {
            return string.Format("{0}:{1}:{2}",
                CreateRowKeyPrefix(observationDateUtc),
                diagnosticServerId.ToString().ToUpperInvariant(),
                connectionName.ToUpperInvariant());
        }

        public static string CreateRowKeyPrefix(DateTime observationDateUtc)
        {
            return (long.MaxValue - observationDateUtc.Date.Ticks).ToString().PadLeft(19, '0');
        }


        #region Persistence

        private const string ResultPropertyPrefix = "Result_";

        public override IDictionary<string, EntityProperty> WriteEntity(OperationContext operationContext)
        {
            var props = base.WriteEntity(operationContext);

            // Default logic does not serialize this as it's read-only
            props.Add("NumHealthCheckResults", EntityProperty.GeneratePropertyForInt(NumHealthCheckResults));

            foreach (var kv in _healthCheckResults)
                props.Add(ResultPropertyPrefix + kv.Key, EntityProperty.GeneratePropertyForDouble(kv.Value));

            return props;
        }

        public override void ReadEntity(IDictionary<string, EntityProperty> properties, OperationContext operationContext)
        {
            base.ReadEntity(properties, operationContext);

            HealthCheckResults = properties
                .Where(kv => kv.Key.StartsWith(ResultPropertyPrefix))
                .ToDictionary(kv => kv.Key.Substring(ResultPropertyPrefix.Length), kv => kv.Value.DoubleValue.Value);
        }

        #endregion


        private static IDictionary<string, T> CopyDictionary<T>(IDictionary<string, T> src)
        {
            return src.ToDictionary(kv => kv.Key, kv => kv.Value);
        }
    }
}
