using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLayer.repository.health_check.ignore
{
    class SqlServerStaticHealthCheckIgnoresRepository
    {
        class SqlServerStaticHealthCheckIgnoresEntity : TableEntity
     {
         public Guid OwnerID { get; set; }
         public Guid DiagnosticServerID { get; set; }
         public Type StaticHealthCheckType { get; set; }
         public string InstanceName { get; set; }
         public string DatabaseName { get; set; }
 
         public SqlServerStaticHealthCheckIgnoresEntity(Guid ownerID, Guid diagnosticServerID, Type healthCheckType, string instanceName, string databaseName)
         {
             OwnerID = ownerID;
             PartitionKey = OwnerID.ToString();
 
             DiagnosticServerID = diagnosticServerID;
             StaticHealthCheckType = healthCheckType;
             RowKey = string.Format("{0}:{1}:{2}:{3}", diagnosticServerID, healthCheckType, instanceName, databaseName);
         }
 
    }
}
}
