using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;
using System.IO;
using System.Data;

namespace DbVersioning {
  public class Db {
    protected string connectionString;
    protected DbProviderFactory dbFactory;
    protected string versionTableName;
    protected string dbName;
    public Db(string connectionString, string providerName, string versionTableName="MetaData") {
      if(string.IsNullOrWhiteSpace(connectionString)) throw new DbVersionException("Connection string required.");
      if(string.IsNullOrWhiteSpace(providerName)) throw new DbVersionException("Provider name required.");
      if(string.IsNullOrWhiteSpace(versionTableName)) throw new DbVersionException("Version table name required.");
      dbName="";
      this.connectionString=connectionString;
      dbFactory=DbProviderFactories.GetFactory(providerName);
      this.versionTableName=versionTableName;
      var dbb=new DbConnectionStringBuilder();
      dbb.ConnectionString=connectionString;
      foreach(string key in dbb.Keys) {
        if(key.ToUpper()=="INITIAL CATALOG") {
          dbName=(string)dbb[key];
          break;
        }
        else if(key.ToUpper()=="DATABASE") {
          dbName=(string)dbb[key];
          break;
        }
      }
      if(dbName=="") throw new DbVersionException(string.Format("'Initial catalog' field not found in connection string '{0}'.", connectionString));
    }


    public DbVersion GetVersion() {
       List<string> tableNames=getExistingTableNames();
      if(tableNames.Contains(versionTableName)) {
        string getVersionCmd=string.Format("SELECT TOP 1 * FROM {0} ORDER BY ID DESC", versionTableName);
        using(DbConnection connection=CreateConnection()) {
          connection.Open();
          var reader=ExecuteQuery(getVersionCmd, connection);
          while(reader.Read()) {
            string version=reader.GetString(reader.GetOrdinal("version"));
            return new DbVersion(version);
          }
          return null;
        }
      }
      else {
        return null;
      }
    }

    private List<string> getExistingTableNames() {
      List<string> tableNames=new List<string>();
     string cmd=string.Format("SELECT TABLE_NAME FROM INFORMATION_SCHEMA.TABLES");
     using(DbConnection connection=CreateConnection()) {
       connection.Open();
       var reader=ExecuteQuery(cmd, connection);
       while(reader.Read()) {
         string name=reader.GetString(reader.GetOrdinal("TABLE_NAME"));
         tableNames.Add(name);
       }        
     }
     return tableNames;
    }
    public DbVersion Initialise() {
      var currentVersion=GetVersion();
      if(currentVersion!=null) throw new DbVersionException(string.Format("A database already exists with version {0}", currentVersion.ToString()));
      using(DbConnection connection=CreateConnection()) {
        connection.Open();
        using(DbTransaction transaction=connection.BeginTransaction()) {
            string createMetaCmd=string.Format("CREATE TABLE {0} (Id INT IDENTITY (1, 1) NOT NULL, Version NVARCHAR(50) NOT NULL, Timestamp DateTime NOT NULL)", versionTableName);
          ExecuteNonQuery(createMetaCmd, connection, transaction);
          setVersion(new DbVersion(0,0), connection, transaction);
          transaction.Commit();
        }
      }
      return GetVersion();
    }
    public void Initialise(IEnumerable<string> sql, DbVersion initialVersion) {
      var currentVersion=GetVersion();
      if(currentVersion!=null) throw new DbVersionException(string.Format("A database already exists with version {0}", currentVersion.ToString()));
      using(DbConnection connection=CreateConnection()) {
        connection.Open();
        using(DbTransaction transaction=connection.BeginTransaction()) {
   
          foreach(var line in sql) {
            ExecuteNonQuery(line, connection, transaction);     
          }
          string createMetaCmd=string.Format("CREATE TABLE {0} (Id INT IDENTITY (1, 1) NOT NULL, Version NVARCHAR(50) NOT NULL, Timestamp DateTime NOT NULL)", versionTableName); 
          ExecuteNonQuery(createMetaCmd, connection, transaction);
          setVersion(initialVersion, connection, transaction);
          transaction.Commit();
        }
      }
    }
    void setVersion(DbVersion version, DbConnection connection, DbTransaction transaction) {
      string setVersionCmd=string.Format("INSERT INTO {0} (Version, Timestamp) VALUES ('{1}', '{2}')", versionTableName, version, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
      ExecuteNonQuery(setVersionCmd, connection, transaction);    

    }
    // Reads sql commands from the StreamReader and runs them on the DB and updates the version.
    // Rolls back on failure
    public void Update(IEnumerable<string> sql, DbVersion oldVersion, DbVersion newVersion) {
      if(newVersion<=oldVersion) throw new ArgumentException("New version must be greater than oldVersion");
      var currentDbVersion=GetVersion();
      if(!currentDbVersion.Equals(oldVersion)) throw new InvalidOperationException(string.Format("The current DB version is {0} but expected {1}", currentDbVersion, oldVersion));      
      using(DbConnection connection=CreateConnection()) {
        connection.Open();
        using(DbTransaction transaction=connection.BeginTransaction()) {

          foreach(var line in sql) {
            ExecuteNonQuery(line, connection, transaction);
          }
          setVersion(newVersion, connection, transaction);
          transaction.Commit();
        }
      }
    }
    public void ExecuteNonQuery(DbCommand cmd) {
    }
    public void ExecuteNonQuery(string commandText, DbConnection connection, DbTransaction transaction) {
      using(DbCommand cmd=CreateCommand(commandText, connection)) {
        ExecuteNonQuery(cmd, connection, transaction);
      }
    }
    public void ExecuteNonQuery(DbCommand cmd, DbConnection connection, DbTransaction transaction) {
      cmd.Connection=connection;
      cmd.Transaction=transaction;
      cmd.ExecuteNonQuery();
    }
    public DbDataReader ExecuteQuery(string commandText, DbConnection connection) {      
      using(DbCommand cmd=CreateCommand(commandText, connection)) {
        return ExecuteQuery(cmd, connection);
      }
    }
    public DbDataReader ExecuteQuery(DbCommand cmd, DbConnection connection) {
      cmd.Connection=connection;  
      return cmd.ExecuteReader();
    }

    private DbCommand CreateCommand(string commandText, DbConnection connection) {
      DbCommand command=dbFactory.CreateCommand();
      command.CommandText=commandText.StripWhiteSpace();
      command.Connection=connection;
      command.CommandType=CommandType.Text;
      return command;
    }

    DbConnection CreateConnection() {
      DbConnection dbConnection=dbFactory.CreateConnection();
      dbConnection.ConnectionString=connectionString;
      return dbConnection;
    }
    
    
    
  }
}
