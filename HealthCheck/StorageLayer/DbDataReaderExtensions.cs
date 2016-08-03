using System.Data.SqlClient;
using System;
using System.Data;
using System.Data.Common;
using System.Numerics;

namespace StorageLayer
{
    public static class DbDataReaderExtensions
    {

        public static T? GetNullable<T>(this IDataReader DbDataReader, int columnIdx)
            where T : struct
        {
            if (DbDataReader.IsDBNull(columnIdx))
                return null;

            return (T)DbDataReader[columnIdx];
        }

        public static T? GetNullable<T>(this IDataReader DbDataReader, string columnName)
            where T : struct
        {
            return DbDataReader.GetNullable<T>(DbDataReader.GetOrdinal(columnName));
        }

        public static DateTime? GetUtcDateTime(this IDataReader DbDataReader, string columnName)
        {
            var result = DbDataReader.GetNullable<DateTime>(columnName);
            if (result.HasValue)
                result = DateTime.SpecifyKind(result.Value, DateTimeKind.Utc);

            return result;
        }

        public static DateTime? GetUtcDateTime(this IDataReader DbDataReader, int columnIdx)
        {
            var result = DbDataReader.GetNullable<DateTime>(columnIdx);
            if (result.HasValue)
                result = DateTime.SpecifyKind(result.Value, DateTimeKind.Utc);

            return result;
        }

        public static double GetDoubleable(this IDataReader DbDataReader, string columnName)
        {
            object value = DbDataReader[columnName];
            return Convert.ToDouble(value);
        }

        public static long GetLongable(this IDataReader DbDataReader, string columnName)
        {
            object value = DbDataReader[columnName];
            return Convert.ToInt64(value);
        }

        public static string GetString(this IDataReader DbDataReader, string columnName)
        {
            try
            {
                object value = DbDataReader[columnName];
                if (value == null || Convert.IsDBNull(value))
                    return null;
                return value.ToString();
            }
            catch (Exception ex)
            {
                ex.Data["columnName"] = string.Format("\"{0}\"", columnName);
                throw;
            }
        }

        public static string GetString(this IDataReader DbDataReader, string columnName, string valueIfNull)
        {
            return GetString(DbDataReader, columnName) ?? valueIfNull;
        }

        public static byte[] GetBytes(this IDataReader DbDataReader, string columnName)
        {
            var columnIdx = DbDataReader.GetOrdinal(columnName);

            if (DbDataReader.IsDBNull(columnIdx))
                return null;

            return (byte[])DbDataReader[columnIdx];
        }

        //public static BigInteger? GetBigInteger(this IDataReader DbDataReader, string columnName)
        //{
        //    var value = DbDataReader.GetString(columnName);
        //    if (string.IsNullOrWhiteSpace(value))
        //        return null;

        //    return BigInteger.Parse(value);
        //}

    }
}