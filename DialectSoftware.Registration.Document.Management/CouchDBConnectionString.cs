using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data.Common;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

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
