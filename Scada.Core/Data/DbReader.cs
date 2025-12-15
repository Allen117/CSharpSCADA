using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;

namespace Scada.Core.Data
{
    public class DbReader
    {
        private readonly string _connectionString;

        public DbReader(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IEnumerable<T> Query<T>(
            string sql,
            Func<SqlDataReader, T> map,
            Action<SqlCommand> parameterize = null)
        {
            using (var conn = new SqlConnection(_connectionString))
            using (var cmd = new SqlCommand(sql, conn))
            {
                if (parameterize != null)
                {
                    parameterize(cmd);
                }

                conn.Open();
                using (var reader = cmd.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        yield return map(reader);
                    }
                }
            }
        }
    }
}
