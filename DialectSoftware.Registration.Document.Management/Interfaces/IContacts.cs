using System;
using System.Collections.Generic;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

namespace DialectSoftware.Registration
{
    public interface IContacts : IEnumerable<Contact>
    {
        Contact GetByEmailAddress(string email);
        IEnumerable<Contact> Next(string key, int count, out int totalRecords);
    }
}
