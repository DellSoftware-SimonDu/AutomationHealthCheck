using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StorageLayer
{
    public class Constants
    {
        public const string CONNECTION_STRING_OPERATIONS = "OperationsConnectionString";

        public enum Type    // Limit these names to 50 characters (so they fit into a SQL Azure table column)
        {
            // [NEW SHC] 1.1 - Add the new SHC type here

            configuration_database_compatibility_level,

            dr_backup,
            dr_simple_recovery_model,

            index_duplicate_index,
            index_fragmented_index,
            index_missing_index,

            io_writelog_wait,

            memory_adhoc_workload_configuration,
            memory_physical_memory_pressure,

            security_guest_access,
            security_password_policy,

            sql_best_practice,
        }

        public enum StaticHealthCheckTypePriority
        {
            low,
            medium,
            high
        }
    }
}
