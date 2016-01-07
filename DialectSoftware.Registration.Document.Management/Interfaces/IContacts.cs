using System;
using System.Collections.Generic;


namespace DialectSoftware.Registration
{
    public interface IContacts : IEnumerable<Contact>
    {
        Contact GetByEmailAddress(string email);
        IEnumerable<Contact> Next(string key, int count, out int totalRecords);
    }
}
