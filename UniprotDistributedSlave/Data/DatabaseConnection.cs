using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

public class BaseDataAccess
{
    protected string ConnectionString { get; set; }

    /// <summary>
    /// Blank constructor
    /// </summary>
    public BaseDataAccess()
    {
    }

    /// <summary>
    /// Constructor with connection string
    /// </summary>
    /// <param name="connectionString"></param>
    public BaseDataAccess(string connectionString)
    {
        this.ConnectionString = connectionString;
    }

    /// <summary>
    /// Creates a new connection of SqlConnection type and returns it after opening.
    /// </summary>
    /// <returns></returns>
    private SqlConnection GetConnection()
    {
        SqlConnection connection = new SqlConnection(this.ConnectionString);
        if (connection.State != ConnectionState.Open)
            connection.Open();
        return connection;
    }

    /// <summary>
    /// Creates a new command of SqlCommand according to specified parameters.
    /// </summary>
    /// <param name="connection"></param>
    /// <param name="commandText"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public SqlCommand GetCommand(DbConnection connection, string commandText)
    {
        SqlCommand command = new SqlCommand(commandText, connection as SqlConnection);
        command.CommandType = CommandType.Text;
        return command;
    }

    /// <summary>
    /// Creates a new parameter of SqlParameter and initialize it with provided value.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="value"></param>
    /// <returns></returns>
    public SqlParameter GetParameter(string parameter, object value)
    {
        SqlParameter parameterObject = new SqlParameter(parameter, value != null ? value : DBNull.Value);
        parameterObject.Direction = ParameterDirection.Input;
        return parameterObject;
    }

    /// <summary>
    /// Creates a new parameter of SqlParameter type with parameter direct set to Output type.
    /// </summary>
    /// <param name="parameter"></param>
    /// <param name="type"></param>
    /// <param name="value"></param>
    /// <param name="parameterDirection"></param>
    /// <returns></returns>
    public SqlParameter GetParameterOut(string parameter, SqlDbType type, object value = null, ParameterDirection parameterDirection = ParameterDirection.InputOutput)
    {
        SqlParameter parameterObject = new SqlParameter(parameter, type); ;

        if (type == SqlDbType.NVarChar || type == SqlDbType.VarChar || type == SqlDbType.NText || type == SqlDbType.Text)
        {
            parameterObject.Size = -1;
        }

        parameterObject.Direction = parameterDirection;

        if (value != null)
        {
            parameterObject.Value = value;
        }
        else
        {
            parameterObject.Value = DBNull.Value;
        }

        return parameterObject;
    }

    /// <summary>
    /// ExecuteNonQuery initializes connection, command, and executes ExecuteNonQuery method of command object. Although the ExecuteNonQuery returns no rows, any output parameters or return values mapped to parameters are populated with data. For UPDATE, INSERT, and DELETE statements, the return value is the number of rows affected by the command. Please refer to MSDN for more details about SqlCommand.ExecuteNonQuery.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public int ExecuteNonQuery(string procedureName, List<DbParameter> parameters)
    {
        int returnValue = -1;

        try
        {
            using (SqlConnection connection = this.GetConnection())
            {
                DbCommand cmd = this.GetCommand(connection, procedureName);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = cmd.ExecuteNonQuery();
            }
        }
        catch (Exception ex)
        {
            LogException("Failed to ExecuteNonQuery for " + procedureName, ex, parameters);
            throw;
        }

        return returnValue;
    }

    /// <summary>
    /// ExecuteScalar initializes connection, command, and executes ExecuteScalar method of command object. Executes the query, and returns the first column of the first row in the result set returned by the query. Additional columns or rows are ignored. Please refer to MSDN for more details about SqlCommand.ExecuteScalar.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <returns></returns>
    public object ExecuteScalar(string procedureName, List<SqlParameter> parameters)
    {
        object returnValue = null;

        try
        {
            using (DbConnection connection = this.GetConnection())
            {
                DbCommand cmd = this.GetCommand(connection, procedureName);

                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                returnValue = cmd.ExecuteScalar();
            }
        }
        catch (Exception ex)
        {
            LogException("Failed to ExecuteScalar for " + procedureName, ex, parameters);
            throw;
        }

        return returnValue;
    }

    /// <summary>
    /// ExecuteReader initializes connection, command and executes ExecuteReader method of command object. Provides a way of reading a forward-only stream of rows from a SQL Server database. We have explicitly omitted using block for connection as we need to return DataReader with open connection state. Now question raises that how will we handle connection close open, for this we have created DataReader with "CommandBehavior.CloseConnection", which means, connection will be closed as related DataReader is closed. Please refer to MSDN for more details about SqlCommand.ExecuteReader and SqlDataReader.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public DbDataReader ExecuteDataReader(string procedureName, List<DbParameter> parameters)
    {
        DbDataReader ds;

        try
        {
            DbConnection connection = this.GetConnection();
            {
                DbCommand cmd = this.GetCommand(connection, procedureName);
                if (parameters != null && parameters.Count > 0)
                {
                    cmd.Parameters.AddRange(parameters.ToArray());
                }

                ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
        catch (Exception ex)
        {
            LogException("Failed to GetDataReader for " + procedureName, ex, parameters);
            throw;
        }

        return ds;
    }

    /// <summary>
    /// ExecuteReader initializes connection, command and executes ExecuteReader method of command object. Provides a way of reading a forward-only stream of rows from a SQL Server database. We have explicitly omitted using block for connection as we need to return DataReader with open connection state. Now question raises that how will we handle connection close open, for this we have created DataReader with "CommandBehavior.CloseConnection", which means, connection will be closed as related DataReader is closed. Please refer to MSDN for more details about SqlCommand.ExecuteReader and SqlDataReader.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <param name="parameters"></param>
    /// <param name="commandType"></param>
    /// <returns></returns>
    public SqlDataReader ExecuteSqlDataReader(string procedureName)
    {
        SqlDataReader ds;

        try
        {
            SqlConnection connection = this.GetConnection();
            {
                SqlCommand cmd = this.GetCommand(connection, procedureName);
                cmd.CommandTimeout = 0;

                ds = cmd.ExecuteReader(CommandBehavior.CloseConnection);
            }
        }
        catch (Exception ex)
        {
            LogException("Failed to GetDataReader for " + procedureName, ex, null);
            throw;
        }

        return ds;
    }


    /// <summary>
    /// Fill Dataset with this function.
    /// </summary>
    /// <param name="procedureName"></param>
    /// <returns></returns>
    public DataSet ExecuteFillDataSet(string procedureName, List<DbParameter> parameters)
    {
        using (DataSet Result = new DataSet())
        {
            try
            {
                SqlConnection connection = this.GetConnection();
                {
                    DbCommand cmd = GetCommand(connection, procedureName);
                    SqlDataAdapter data = new SqlDataAdapter(procedureName, connection);
                    if (parameters != null && parameters.Count > 0)
                    {
                        cmd.Parameters.AddRange(parameters.ToArray());
                    }
                    data.Fill(Result);
                    return Result;
                }
            }
            catch (Exception ex)
            {
                LogException("Failed to ExecuteFillDataSet for " + procedureName, ex, parameters);
                throw;
            }
        }
    }

    /// <summary>
    /// Exception Log
    /// </summary>
    /// <param name="v"></param>
    /// <param name="ex"></param>
    /// <param name="parameters"></param>
    private void LogException(string v, Exception ex, object parameters)
    {
        Console.WriteLine(v, ex.Message, parameters);
    }
}