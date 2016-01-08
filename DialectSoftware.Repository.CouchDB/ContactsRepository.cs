using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Runtime.Serialization;
using DialectSoftware.Registration.Repository;
using DialectSoftware.Registration;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

namespace DialectSoftware.Registration.Repository
{
    public class ContactsRepository:IContactRepository 
    {
        Contacts contacts;
        
        public ContactsRepository()
        {
            object server;
            object database;
            object user;
            object password;
            var connectionString = new CouchDBConnectionString(System.Configuration.ConfigurationManager.ConnectionStrings["Contacts"].ConnectionString);
            connectionString.TryGetValue(CouchDBConnectionString.Server, out server);
            connectionString.TryGetValue(CouchDBConnectionString.Database, out database);
            connectionString.TryGetValue(CouchDBConnectionString.Username, out user);
            connectionString.TryGetValue(CouchDBConnectionString.Password, out password);
            SharpCouch.DB db = new SharpCouch.DB(server.ToString(), user.ToString(), password.ToString());
            contacts = Contacts.Connect(db, database.ToString());
        }

        public Contact Read(string id)
        {
            return contacts[id];
        }

        public void Create(Contact item)
        {
            contacts.Add(item);
        }

        public void Update(string id, Contact item)
        {
            contacts[id] = item;
        }

        public void Delete(string id)
        {
            contacts.Remove(id);
        }

        public Contact GetByEmailAddress(string email)
        {
            return contacts.GetByEmailAddress(email);
        }

        public IEnumerable<Contact> Next(string id, int count, out int totalRecords)
        {
            return contacts.Next(id, count, out totalRecords);
        }

        public IEnumerator<Contact> GetEnumerator()
        {
            return contacts.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void Initialize()
        {
            contacts.Create();
        }
    }
}
