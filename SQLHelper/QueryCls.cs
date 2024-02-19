using Microsoft.Extensions.Configuration;
using R_Security.Encryption;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Data.Linq;
using System.Linq;

namespace SQLHelper
{
    public class QueryCls
    {
        private const string DEFAULT_CONNECTION_NAME = "DefaultConnectionString";
        private const string K3Y54LT = "K3YR34LT43861772_K3YRND101601778";
        private const string DEFAULT_PROVIDER = "System.Data.SqlClient";

        private readonly IConfiguration _configuration;
        private int _commandTimeout = 600;

        public QueryCls()
        {
            _configuration = ConfigurationUtility.GetConfiguration();
        }

        #region SqlExecQuery
        public DataTable SqlExecQuery(string query, DbConnection dbConnection = null, bool autoCloseConnection = true, int commandTimeout = 0)
        {
            try
            {
                var connection = dbConnection;
                if (dbConnection == null)
                    connection = GetConnection();

                var command = connection.CreateCommand();
                command.CommandText = query;
                command.CommandTimeout = commandTimeout > 0 ? commandTimeout : _commandTimeout;

                return SqlExecQuery(connection, command, autoCloseConnection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public DataTable SqlExecQuery(DbConnection dbConnection, DbCommand dbCommand, bool autoCloseConnection = false)
        {
            DataTable dataTable = new DataTable();
            DbDataReader dataReader = null;

            try
            {
                dbCommand.Connection = dbConnection;

                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();

                dataReader = dbCommand.ExecuteReader();
                dataTable.Load(dataReader, LoadOption.OverwriteChanges);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (dataReader != null && !dataReader.IsClosed)
                    dataReader.Close();

                if (autoCloseConnection && dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed)
                {
                    dbConnection.Close();
                    dbConnection.Dispose();
                }
            }

            return dataTable;
        }
        #endregion

        #region SqlExecObjectQuery
        public List<T> SqlExecObjectQuery<T>(string query, params object[] parameter)
        {
            try
            {
                return SqlExecObjectQuery<T>(query, GetConnection(), true, GetCommandTimeoutDefault(), parameter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> SqlExecObjectQuery<T>(string query, DbConnection dbConnection, bool autoCloseConnection, params object[] parameter)
        {
            try
            {
                return SqlExecObjectQuery<T>(query, dbConnection, autoCloseConnection, GetCommandTimeoutDefault(), parameter);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public List<T> SqlExecObjectQuery<T>(string query, DbConnection dbConnection, bool autoCloseConnection, int commandTimeout, params object[] parameter)
        {
            List<T> result = null;

            try
            {
                if (dbConnection.State != System.Data.ConnectionState.Open)
                    dbConnection.Open();

                var dataContext = new DataContext(dbConnection);
                dataContext.CommandTimeout = commandTimeout;

                result = dataContext.ExecuteQuery<T>(query, parameter).ToList();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (autoCloseConnection && dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed)
                {
                    dbConnection.Close();
                    dbConnection.Dispose();
                }
            }

            return result;
        }
        #endregion

        #region SqlExecNonQuery
        public int SqlExecNonQuery(string query, DbConnection dbConnection = null, bool autoCloseConnection = true, int commandTimeout = 0)
        {
            try
            {
                var connection = dbConnection;
                if (dbConnection == null)
                    connection = GetConnection();

                var dbCommand = connection.CreateCommand();
                dbCommand.CommandText = query;
                dbCommand.CommandTimeout = commandTimeout > 0 ? commandTimeout : _commandTimeout;

                return SqlExecNonQuery(connection, dbCommand, autoCloseConnection);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public int SqlExecNonQuery(DbConnection dbConnection, DbCommand dbCommand, bool autoCloseConnection = false)
        {
            try
            {
                dbCommand.Connection = dbConnection;
                if (dbConnection.State != ConnectionState.Open)
                    dbConnection.Open();

                return dbCommand.ExecuteNonQuery();
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                if (autoCloseConnection && dbConnection != null && dbConnection.State != System.Data.ConnectionState.Closed)
                {
                    dbConnection.Close();
                    dbConnection.Dispose();
                }
            }
        }
        #endregion

        #region GetConnection
        public DbConnection GetConnection(string connectionString, string provider)
        {
            DbConnection dbConnection = null;

            try
            {
                //Set ConnectionString
                string dbConnectionString = connectionString.Trim();
                if (string.IsNullOrWhiteSpace(connectionString))
                    dbConnectionString = GetConnectionString();

                string dbProvider = provider.Trim();
                if (string.IsNullOrWhiteSpace(provider))
                    dbProvider = GetProvider();

                //Create Connection
                dbConnection = DbProviderFactories.GetFactory(dbProvider).CreateConnection();
                dbConnection.ConnectionString = dbConnectionString;
            }
            catch (Exception)
            {
                throw;
            }

            return dbConnection;
        }

        public DbConnection GetConnection(ConnectionAttribute connectionAttribute)
        {
            var connectionString = string.IsNullOrWhiteSpace(connectionAttribute.ConnectionString) ? "" : connectionAttribute.ConnectionString;
            var provider = string.IsNullOrWhiteSpace(connectionAttribute.Provider) ? "" : connectionAttribute.Provider;

            return GetConnection(connectionString, provider);
        }

        public DbConnection GetConnection(string connectionName = "")
        {
            return GetConnection(GetConnectionAttribute(connectionName));
        }
        #endregion

        #region ConnectionString
        public ConnectionAttribute GetConnectionAttribute(string connectionName = "")
        {
            ConnectionAttribute result = default;

            try
            {
                if (string.IsNullOrWhiteSpace(connectionName))
                    connectionName = DEFAULT_CONNECTION_NAME;

                var connectionString = _configuration.GetConnectionString(connectionName);

                //decrypt password
                var connectionStringArr = connectionString.Split(";");
                var passwordString = connectionStringArr.Where(x => x.StartsWith("Password", StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault();
                if (string.IsNullOrWhiteSpace(passwordString))
                    throw new Exception("Password on connection string not found");

                string password = "";
                var passwordArr = passwordString.Split("=");
                if (passwordArr.Count() > 1)
                    password = passwordArr[1];
                else
                    password = passwordArr[0];

                var decryptPassword = Decrypt(password.Trim(), K3Y54LT);
                var newPassword = "Password=" + decryptPassword.Trim();

                result.ConnectionString = connectionString.Replace(passwordString, newPassword);
                result.Provider = DEFAULT_PROVIDER;
            }
            catch (Exception)
            {
                throw;
            }

            return result;
        }

        public int GetCommandTimeoutDefault()
        {
            return _commandTimeout;
        }

        private string GetConnectionString()
        {
            var connectionAttr = GetConnectionAttribute(DEFAULT_CONNECTION_NAME);

            return connectionAttr.ConnectionString;
        }

        private string GetProvider()
        {
            var connectionAttr = GetConnectionAttribute(DEFAULT_CONNECTION_NAME);

            return connectionAttr.Provider;
        }

        private string Decrypt(string pcData, string pcKey)
        {
            try
            {
                var sym = new Symmetric(Symmetric.Provider.Rijndael);
                var key = new Data(pcKey.Trim().ToUpper());
                var encryptedData = new Data()
                {
                    Hex = pcData.Trim()
                };

                var decryptedData = sym.Decrypt(encryptedData, key);

                return decryptedData.ToString();
            }
            catch (Exception)
            {
                throw;
            }
        }
        #endregion
    }
}
