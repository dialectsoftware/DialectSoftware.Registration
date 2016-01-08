using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

/// ******************************************************************************************************************
/// * Copyright (c) 2011 Dialect Software LLC                                                                        *
/// * This software is distributed under the terms of the Apache License http://www.apache.org/licenses/LICENSE-2.0  *
/// *                                                                                                                *
/// ******************************************************************************************************************

namespace DialectSoftware.Registration
{
    //[Serializable]
    [XmlRoot("property")]
    public class Property
    {
        public virtual string category
        {
            get;
            set;
        }

        public virtual string name
        {
            get;
            set;
        }

        public virtual string mimeType
        {
            get;
            set;
        }

        public virtual string propertyValueString
        {
            get;
            set;
        }

        public virtual byte[] propertyValueBinary
        {
            get;
            set;
        }

        public virtual System.DateTime lastUpdatedDate
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj is Property && ((Property)obj).category == category && ((Property)obj).name == name;
        }

        public override int GetHashCode()
        {
            return (category??"").GetHashCode() ^ (name??"").GetHashCode();
        }
    }
}
