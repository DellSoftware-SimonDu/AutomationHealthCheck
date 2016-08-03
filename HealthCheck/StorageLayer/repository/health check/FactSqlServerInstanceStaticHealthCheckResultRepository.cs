using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLayer.repository
{
    public class FactSqlServerInstanceStaticHealthCheckResultRepository
    {
        public static string DownloadLatestStaticHealthCheckResultJsonFromBlobStore(long ownerID, Guid dsID, string connectionName)
        {
            var blobName = GenerateBlobNames3(ownerID, dsID, connectionName, 2).Single();
            try
            {
                var json = new BlobStorage().TryReadString(BlobStorage.SQLSERVER_STATIC_HEALTH_CHECK_CONTAINER, blobName);
                return json;
            }
            catch (Exception ex)
            {
                ex.Data["blobName"] = blobName;
                throw;
            }
        }

        /// <remarks>PL-6103</remarks>
        private static string[] GenerateBlobNames3(long ownerID, Guid diagnosticServerID, string connectionName, int overallHealthcheckVersion, DateTime? datePerformed = null)
        {
            var blobNames = new List<string>();
            if (datePerformed.HasValue)
            {
                blobNames.Add(datePerformed.Value.ToString("yyyy-MM-ddTHH:mm:ssZ")); // Unique filename that's kept forever
            }
            blobNames.Add("LATEST"); // Fixed filename so UI can find it cheaply
            // @gmar - ok to record as 'LATEST' (=shown in UI) even if it is a rerun of yesterday or whatever.
            // we don't think that's a likely scenario to worry about.

            return blobNames.Select(blobName =>
                                string.Format("{0}/{1}/{2}/{3}/{4}.json"
                                            , OwnerRepository.OwnerIDToUserID(ownerID).Value
                                            , overallHealthcheckVersion
                                            , diagnosticServerID
                                            , connectionName // @pokeeffe - No need to encode the connection name for blob store
                                            , blobName
                                            )).ToArray();
        }
    }

    public class FactSqlServerInstanceStaticHealthCheckResult
    {
        public Guid ID { get; set; }
        public long OwnerID { get; set; }
        public DateTime ObservationTime { get; set; }
        public DateTime DatePerformed { get; set; }

        public int HealthCheckScoreVersion { get; set; }
        public double HealthCheckScore { get; set; }        //<- Excludes ignored SHC types at the time the SHC was performed

        public int? StaticHealthCheckCount { get; set; }    //<- Excludes ignored SHC types at the time the SHC was performed
        public int? HighPriorityCount { get; set; }         //<- Excludes ignored SHC types at the time the SHC was performed
        public int? MediumPriorityCount { get; set; }       //<- Excludes ignored SHC types at the time the SHC was performed
        public int? LowPriorityCount { get; set; }          //<- Excludes ignored SHC types at the time the SHC was performed
        public Guid DiagnosticServerID { get; set; }
        public string ConnectionName { get; set; }
        public string InstanceName { get; set; }
        public bool IsHaveStaticHealthCheckResultJson { get; set; }
    }
}
