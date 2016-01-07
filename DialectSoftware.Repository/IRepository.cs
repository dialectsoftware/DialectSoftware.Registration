using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

namespace DialectSoftware.Storage.Repository
{
    public interface IRepository<T>
    {
        T Read(string key);
        void Create(T item);
        void Update(string key, T item);
        void Delete(string key);
        void Initialize();
    }
}
