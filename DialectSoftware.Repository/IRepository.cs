using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

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
