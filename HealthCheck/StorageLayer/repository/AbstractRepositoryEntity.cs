using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Data.SqlClient;
using System.Data.Common;

namespace StorageLayer.repository
{
    public abstract class AbstractRepositoryEntity : ICloneable
    {
        public virtual void InitializeFromRow(IDataReader aReader)
        {
            throw new NotImplementedException();
        }

        protected long? GetNullableLong(DbDataReader aReader, string fieldName)
        {
            if (aReader.IsDBNull(aReader.GetOrdinal(fieldName)))
                return null;
            else
                return (long)aReader[fieldName];
        }

        object ICloneable.Clone()
        {
            return this.MemberwiseClone();
        }

        public interface IRecoverableDimensionTableEntity
        {
            string GetSqlInsertStatement();
        }

        public static string GetDimensionTableDataRecoverySqlScript(bool disableAutoID, string databaseName, string tableName, IEnumerable<AbstractRepositoryEntity.IRecoverableDimensionTableEntity> entities)
        {
            /*
             * This code may be used if this table ever needs to be restored from a backup and data has been lost.
             * This code inserts the rows for this table that were added *after* the backup was taken.
             */

            var result = new StringBuilder();

            var fullTableName = (string.IsNullOrWhiteSpace(databaseName) ? "" : databaseName + ".") + tableName;
            var fullDboTableName = string.Format("{0}dbo.{1}"
                                                , (string.IsNullOrWhiteSpace(databaseName) ? "" : databaseName + ".")
                                                , tableName
                                                );

            result.AppendLine("--".PadRight(50, '*'));
            result.AppendLine("--  [" + fullTableName + "]");
            result.AppendLine("--".PadRight(50, '*'));

            if (!entities.Any())
                result.AppendLine("--\tNo dimension-ids need to be recovered for this table.");
            else
            {
                if (disableAutoID)
                    result.AppendLine(string.Format("\tSET IDENTITY_INSERT {0} ON;"
                                                    , fullDboTableName
                                                    ));

                foreach (var entity in entities)
                {
                    result.AppendLine("\t" + entity.GetSqlInsertStatement());
                }

                if (disableAutoID)
                    result.AppendLine(string.Format("\tSET IDENTITY_INSERT {0} OFF;"
                                                    , fullDboTableName
                                                    ));
            }
            return result.ToString();
        }
    }

    public abstract class ScalarRepositoryEntity<T> : AbstractRepositoryEntity
    {
        public T Value { get; set; }
    }

    public class ScalarIntEntity : ScalarRepositoryEntity<long>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? 0 : (aReader[0] is long) ? (long)aReader[0] : (int)aReader[0];
        }
    }

    public class ScalarNullableIntEntity : ScalarRepositoryEntity<int?>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            if (aReader[0] != DBNull.Value)
                Value = (int)aReader[0];
        }
    }

    public class ScalarNullableLongEntity : ScalarRepositoryEntity<long?>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            if (aReader[0] != DBNull.Value)
                Value = (long)aReader[0];
        }
    }

    public class ScalarDecimalEntity : ScalarRepositoryEntity<long>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? 0 : Convert.ToInt64((decimal)aReader[0]);
        }
    }

    public class ScalarDoubleEntity : ScalarRepositoryEntity<double>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? 0 : (double)aReader[0];
        }
    }

    public class ScalarNullableDoubleEntity : ScalarRepositoryEntity<double?>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            if (aReader[0] != DBNull.Value)
                Value = (double)aReader[0];
        }
    }

    public class ScalarStringEntity : ScalarRepositoryEntity<string>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? "" : (string)aReader[0];
        }
    }

    public class ScalarBooleanEntity : ScalarRepositoryEntity<bool>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? false : Convert.ToInt32(aReader[0]) == 1;
        }
    }

    public class ScalarGuidEntity : ScalarRepositoryEntity<Guid>
    {
        public override void InitializeFromRow(System.Data.IDataReader aReader)
        {
            Value = aReader[0] == DBNull.Value ? Guid.Empty : (Guid)aReader[0];
        }
    }

}
