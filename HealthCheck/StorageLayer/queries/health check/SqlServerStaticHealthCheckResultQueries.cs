using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using StorageLayer.repository;
using StorageLayer.tablestore;
using Newtonsoft.Json.Linq;

namespace StorageLayer.queries.healthcheck
{
    public class SqlServerStaticHealthCheckResultQueries
    {
        public static FactSqlServerInstanceStaticHealthCheckResult GetMostRecentInstanceStaticHealthCheck(OwnerEntity owner, Guid diagnosticServerID, string connectionName, string instanceName)
        {
            var fact = new FactSqlServerInstanceStaticHealthCheckResult();

            fact.InstanceName = instanceName;
            fact.DiagnosticServerID = diagnosticServerID;
            fact.ConnectionName = connectionName;

            var json = FactSqlServerInstanceStaticHealthCheckResultRepository.
                       DownloadLatestStaticHealthCheckResultJsonFromBlobStore(owner.ID, diagnosticServerID, connectionName);

            if (json == null)
            {
                fact.IsHaveStaticHealthCheckResultJson = false;
                fact.ObservationTime = DateTime.MinValue;
                fact.HighPriorityCount = 0;
                fact.MediumPriorityCount = 0;
                fact.LowPriorityCount = 0;

                return fact;
            }

            fact.IsHaveStaticHealthCheckResultJson = true;
            var healthCheck = JObject.Parse(json);
            fact.DatePerformed = (DateTime)healthCheck["DatePerformed"];
            var observationTime = (DateTime)healthCheck["Date"];
            fact.ObservationTime = observationTime;
            var context = new SystemHealthCheckResultContext();

            var systemHealthCheckResultEntity = context.Select(owner.UserId, observationTime, diagnosticServerID, connectionName);

            if (systemHealthCheckResultEntity != null)
            {
                fact.HighPriorityCount = systemHealthCheckResultEntity.HighPriorityCount;
                fact.MediumPriorityCount = systemHealthCheckResultEntity.MediumPriorityCount;
                fact.LowPriorityCount = systemHealthCheckResultEntity.LowPriorityCount;
            }
            else
            {
                fact.HighPriorityCount = 0;
                fact.MediumPriorityCount = 0;
                fact.LowPriorityCount = 0;
            }

            return fact;
        }

        public class HealthCheckSummary
        {
            public long OwnerID { get; set; }
            public List<FactSqlServerInstanceStaticHealthCheckResult> Facts { get; set; }

            public int InstanceCount
            {
                get
                {
                    return Facts.Select(f => new { f.DiagnosticServerID, f.ConnectionName }).Distinct().Count();
                }
            }

            public int CountOfInstancesWithHighPriorityIssues
            {
                get
                {
                    return CountOfInstancesWithPriority(Constants.StaticHealthCheckTypePriority.high);
                }
            }

            public int CountOfInstancesWithMediumPriorityIssues
            {
                get
                {
                    return CountOfInstancesWithPriority(Constants.StaticHealthCheckTypePriority.medium);
                }
            }

            public int CountOfInstancesWithLowPriorityIssues
            {
                get
                {
                    return CountOfInstancesWithPriority(Constants.StaticHealthCheckTypePriority.low);
                }
            }

            public int CountOfInstancesWithoutIssues
            {
                get
                {
                    return Facts.Count(f => f.HighPriorityCount == 0 &&
                                            f.MediumPriorityCount == 0 &&
                                            f.LowPriorityCount == 0);
                }
            }

            public IEnumerable<FactSqlServerInstanceStaticHealthCheckResult> GetResultsByPriority(
                Constants.StaticHealthCheckTypePriority priority)
            {
                return Facts.Where(GetPredicate(priority));
            }

            private int CountOfInstancesWithPriority(Constants.StaticHealthCheckTypePriority priority)
            {
                return Facts.Count(GetPredicate(priority));
            }

            private static Func<FactSqlServerInstanceStaticHealthCheckResult, bool> GetPredicate(
                Constants.StaticHealthCheckTypePriority priority)
            {
                switch (priority)
                {
                    case Constants.StaticHealthCheckTypePriority.low:
                        return (f => f.HighPriorityCount == 0 &&
                                     f.MediumPriorityCount == 0 &&
                                     f.LowPriorityCount > 0);
                    case Constants.StaticHealthCheckTypePriority.medium:
                        return (f => f.HighPriorityCount == 0 &&
                                     f.MediumPriorityCount > 0);
                    case Constants.StaticHealthCheckTypePriority.high:
                        return (f => f.HighPriorityCount > 0);
                    default:
                        throw new NotImplementedException();
                }
            }
        }
    }
}
