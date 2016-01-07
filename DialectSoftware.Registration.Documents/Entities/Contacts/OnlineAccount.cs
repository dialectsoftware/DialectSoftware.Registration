using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DialectSoftware.Registration
{
    //[Serializable]
    [XmlRoot("onlineAccount")]
    public class OnlineAccount
    {
        public int accountType
        {
            get;
            set;
        }

        public virtual string uri
        {
            get;
            set;
        }

        public virtual string service
        {
            get;
            set;
        }

        public virtual string host
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj is OnlineAccount && ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            return (uri??"").ToString();
        }
    }
}
