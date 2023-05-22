using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using System.Threading.Tasks;

namespace CPToolServerSide.Database
{
    public class SqlUtil
    {
        private string ConnectionString { get; }
        public SqlUtil(string connection)
        {
            ConnectionString = "";

            string[] tokens = connection.Split(';');
            foreach (var token in tokens)
            {
                if (token.Trim().ToLowerInvariant().StartsWith("provider"))
                    continue;

                if (!String.IsNullOrEmpty(ConnectionString))
                    ConnectionString += ';';

                ConnectionString += token;
            }
        }

        public int Execute(string query, IEnumerable<SqlParameter> parameters = null)
        {
            int iRet = -1;

            using (var connection = new SqlConnection(ConnectionString))
            {
                using var command = new SqlCommand(query, connection);
                AddCommandParameters(command, parameters);
                command.Connection.Open();

                iRet = command.ExecuteNonQuery();

                command.Parameters.Clear();
            }

            return iRet;
        }

        public T GetValue<T>(string query, IEnumerable<SqlParameter> parameters = null)
        {
            try
            {
                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection);
                AddCommandParameters(command, parameters);
                command.Connection.Open();

                var val = command.ExecuteScalar();
                command.Parameters.Clear();

                return val is DBNull || val == null ? default : (T)val;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                return default;
            }
        }

        public DataTable GetResultSet(string query, IEnumerable<SqlParameter> parameters = null)
        {
            var dt = new DataTable();
            try
            {
                var sda = new SqlDataAdapter();

                using var connection = new SqlConnection(ConnectionString);
                using var command = new SqlCommand(query, connection)
                {
                    CommandTimeout = 3600  // in seconds
                };
                AddCommandParameters(command, parameters);

                sda.SelectCommand = command;
                sda.Fill(dt);
                command.Parameters.Clear();
                return dt;
            }
            catch (Exception)
            {
                throw;
            }
        }

        private static void AddCommandParameters(SqlCommand command, IEnumerable<SqlParameter> parameters)
        {
            if (parameters == null)
            {
                return;
            }

            foreach (SqlParameter p in parameters)
            {
                if (p == null)
                {
                    continue;
                }

                if (p.Value == null)
                {
                    var clone = (SqlParameter)((ICloneable)p).Clone();
                    clone.Value = DBNull.Value;
                    command.Parameters.Add(clone);
                }
                else
                {
                    command.Parameters.Add(p);
                }
            }
        }
    }
}

