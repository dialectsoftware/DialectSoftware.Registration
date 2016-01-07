using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DialectSoftware.Registration
{
    //[Serializable]
    [XmlRoot("privilege")]
    public class Privilege
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

        public virtual int code
        {
            get;
            set;
        }

        public virtual string description
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj is Privilege && ((Privilege)obj).category == category && ((Privilege)obj).name == name && ((Privilege)obj).code == code;
        }

        public override int GetHashCode()
        {
            return (category??"").GetHashCode() ^ (name??"").GetHashCode() ^ code.GetHashCode();
        }
    }
}
