using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

namespace DialectSoftware.Registration
{
    public static class Extensions
    {
        public static StringBuilder AppendOnly(this StringBuilder sb, string value)
        {
            if (value != null)
                sb.AppendLine(value);
            return sb;
        }
    }
}
