using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DbVersioning;
using System.IO;

namespace DbVersionTestConsole {
  class Program {
    static void Main(string[] args) {
      try {
        Db db=new Db(@"Data Source=.\SQLEXPRESS;Initial Catalog=TEST;integrated security=SSPI;",
          "System.Data.SqlClient");

        // Each string in the collection must be a complete SQL command.

        List<string> sqlCommands=new List<string>();
        sqlCommands.Add("CREATE TABLE Table1 (Id INT IDENTITY (1, 1) NOT NULL, Name NVARCHAR(50) NOT NULL) ON [PRIMARY]");
        sqlCommands.Add("CREATE TABLE Table2 (Id INT IDENTITY (1, 1) NOT NULL, Description NVARCHAR(50) NOT NULL) ON [PRIMARY]");

        db.Initialise(sqlCommands, new DbVersion());

        DbVersion currentVersion=db.GetVersion();
        Console.WriteLine("Current DB version: {0}", currentVersion);

        sqlCommands=new List<string>();
        sqlCommands.Add("CREATE TABLE Table3 (Id INT IDENTITY (1, 1) NOT NULL, Name NVARCHAR(50) NOT NULL) ON [PRIMARY]");

        db.Update(sqlCommands, currentVersion, new DbVersion("1.1"));

        Console.WriteLine("Current DB version: {0}", db.GetVersion());
      }
      catch(Exception ex) {
        Console.WriteLine("EXCEPTION: {0}", ex.Message);
      }

    }
  }
}
