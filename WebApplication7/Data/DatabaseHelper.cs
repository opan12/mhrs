using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

public class DatabaseHelper
{
    private readonly string _connectionString;

    public DatabaseHelper(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("DefaultConnection");
    }

    public DataTable ExecuteQuery(string query, CommandType commandType = CommandType.Text, SqlParameter[] parameters = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                var dataTable = new DataTable();
                var dataAdapter = new SqlDataAdapter(command);
                dataAdapter.Fill(dataTable);
                return dataTable;
            }
        }
    }

    public int ExecuteNonQuery(string query, CommandType commandType = CommandType.Text, SqlParameter[] parameters = null)
    {
        using (var connection = new SqlConnection(_connectionString))
        {
            connection.Open();
            using (var command = new SqlCommand(query, connection))
            {
                command.CommandType = commandType;
                if (parameters != null)
                {
                    command.Parameters.AddRange(parameters);
                }

                return command.ExecuteNonQuery();
            }
        }
    }
}
