using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DialectSoftware.Registration;
using DialectSoftware.Storage.Repository;

namespace DialectSoftware.Registration.Repository
{
    public interface IContactRepository : IContacts, IRepository<Contact>
    {
    }
}
