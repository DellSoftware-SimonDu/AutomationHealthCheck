using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;

namespace StorageLayer.tablestore
{
    public static class TableQueryExtenstions
    {
        public static IEnumerable<T> ExecuteSegmentedOn<T>(this TableQuery<T> query, CloudTable table, TableRequestOptions tableRequestOptions = null)
            where T : TableEntity, new()
        {
            TableContinuationToken token = null;
            TableQuerySegment<T> segment;

            do
            {
                segment = table.ExecuteQuerySegmented(query, token, tableRequestOptions);

                foreach (var result in segment)
                {
                    yield return result;
                }

                token = segment.ContinuationToken;

            } while (token != null);
        }

        /// <summary>
        /// Convert object value to EntityProperty.
        /// If it's not a primitive type, the ToString() 
        /// of the object will be called, so make sure
        /// the returned string is meaningful.
        /// </summary>
        public static EntityProperty ToEntityProperty(this object value, string key)
        {
            EntityProperty result = null;

            if (value == null)
                result = new EntityProperty((string)null);
            if (value.GetType() == typeof(byte[]))
                result = new EntityProperty((byte[])value);
            if (value.GetType() == typeof(bool))
                result = new EntityProperty((bool)value);
            if (value.GetType() == typeof(DateTimeOffset))
                result = new EntityProperty((DateTimeOffset)value);
            if (value.GetType() == typeof(DateTime))
                result = new EntityProperty((DateTime)value);
            if (value.GetType() == typeof(double))
                result = new EntityProperty((double)value);
            if (value.GetType() == typeof(Guid))
                result = new EntityProperty((Guid)value);
            if (value.GetType() == typeof(int))
                result = new EntityProperty((int)value);
            if (value.GetType() == typeof(long))
                result = new EntityProperty((long)value);
            if (value.GetType() == typeof(string))
                result = new EntityProperty((string)value);

            if (result == null)
            {
                result = new EntityProperty(value.ToString());
            }

            return result;

        }
    }
}
