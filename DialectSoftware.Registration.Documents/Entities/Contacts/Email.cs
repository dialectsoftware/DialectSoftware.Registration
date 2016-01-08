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
    [XmlRoot("email")]
    public class Email
    {
        #region Primitive Properties

        public virtual int id
        {
            get;
            set;
        }

        public virtual int emailType
        {
            get;
            set;
        }
      

        public virtual string formattedEmailAddress
        {
            get;
            set;
        }

        public virtual string localpart
        {
            get;
            set;
        }

        public virtual string domainName
        {
            get;
            set;
        }

        public virtual string extension
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj is Email && ((Email)obj).ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return String.Format("{0}@{1}.{2}",localpart??"",domainName??"",extension??"");
        }

        #endregion
    }
}
