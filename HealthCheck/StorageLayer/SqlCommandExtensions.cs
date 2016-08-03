using System.Data.SqlClient;
using System;
using System.Data.Common;
using System.Data;

namespace StorageLayer
{
    public static class SqlCommandExtensions
    {
        public static void SetParameter(this IDbCommand dbCommand, string parameterName, string parameterValue)
        {
            if (string.IsNullOrEmpty(parameterValue))
                dbCommand.Parameters.Add(new SqlParameter(parameterName, DBNull.Value));
            else
                dbCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, Guid parameterValue)
        {
            dbCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, Guid? parameterValue)
        {
            if (parameterValue == null)
                dbCommand.Parameters.Add(new SqlParameter(parameterName, DBNull.Value));
            else
                dbCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, double? parameterValue)
        {
            SetParameter<double>(dbCommand, parameterName, parameterValue);
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, int? parameterValue)
        {
            SetParameter<int>(dbCommand, parameterName, parameterValue);
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, long? parameterValue)
        {
            SetParameter<long>(dbCommand, parameterName, parameterValue);
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, DateTime? parameterValue)
        {
            SetParameter<DateTime>(dbCommand, parameterName, parameterValue);
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, bool? parameterValue)
        {
            SetParameter<bool>(dbCommand, parameterName, parameterValue);
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, char? parameterValue)
        {
            SetParameter<char>(dbCommand, parameterName, parameterValue);
        }

        private static void SetParameter<T>(IDbCommand dbCommand, string parameterName, T? parameterValue)
            where T : struct
        {
            if (parameterValue.HasValue)
                dbCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue.Value));
            else
                dbCommand.Parameters.Add(new SqlParameter(parameterName, DBNull.Value));
        }

        public static void SetParameter(this IDbCommand dbCommand, string parameterName, byte[] parameterValue)
        {
            if (parameterValue != null)
                dbCommand.Parameters.Add(new SqlParameter(parameterName, parameterValue));
            else
            {
                var p = new SqlParameter(parameterName, SqlDbType.Binary) { Value = DBNull.Value };
                dbCommand.Parameters.Add(p);
            }
        }
    }
}