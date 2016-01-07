using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

namespace DialectSoftware.Registration.Repository
{
    public class CouchDBConnectionString : DbConnectionStringBuilder
    {
        public const string Server = "Server";
        public const string Database = "db";
        public const string Username = "username";
        public const string Password = "password";

        public CouchDBConnectionString():base(false)
        {
        
        }


        public CouchDBConnectionString(string connectionString)
            : base(false)
        {
            ConnectionString = connectionString;
        }
    }
}
