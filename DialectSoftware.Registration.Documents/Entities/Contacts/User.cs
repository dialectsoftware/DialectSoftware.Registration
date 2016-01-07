using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.Xml.Serialization;

namespace DialectSoftware.Registration
{
    //[Serializable]
    [XmlRoot("user")]
    public class User:IDeserializationCallback
    {
        public User()
        {
            properties = new Property[] { };
            privileges = new Privilege[] { };
        }

        #region Primitive Properties

        public string _id
        {
            get 
            {
                return ToString(); // String.Format("{0}@{1}", userName ?? "", realm ?? "tempuri.org");  
            }
        }

        public string type
        {
            get { return "User"; }
        }

        public virtual string userName
        {
            get;
            set;
        }

        public virtual string realm
        {
            get;
            set;
        }

        public virtual bool isConfirmed
        {
            get;
            set;
        }

        public virtual bool isAnonymous
        {
            get;
            set;
        }

        public virtual string password
        {
            get;
            set;
        }

        public virtual int passwordFormat
        {
            get;
            set;
        }
     
        public virtual string passwordSalt
        {
            get;
            set;
        }

        public virtual string mobileAlias
        {
            get;
            set;
        }

        public virtual string mobilePIN
        {
            get;
            set;
        }

        public virtual string email
        {
            get;
            set;
        }

        public virtual string passwordQuestion
        {
            get;
            set;
        }

        public virtual string passwordAnswer
        {
            get;
            set;
        }

        public virtual System.DateTime lastActivityDate
        {
            get;
            set;
        }

        public virtual bool isLockedOut
        {
            get;
            set;
        }

        public virtual System.DateTime createDate
        {
            get;
            set;
        }

        public virtual System.DateTime lastLoginDate
        {
            get;
            set;
        }

        public virtual System.DateTime lastPasswordChangedDate
        {
            get;
            set;
        }

        public virtual System.DateTime lastLockoutDate
        {
            get;
            set;
        }

        public virtual int failedPasswordAttemptCount
        {
            get;
            set;
        }

        public virtual System.DateTime failedPasswordAttemptWindowStart
        {
            get;
            set;
        }

        public virtual int failedPasswordAnswerAttemptCount
        {
            get;
            set;
        }

        public virtual System.DateTime failedPasswordAnswerAttemptWindowStart
        {
            get;
            set;
        }

        public virtual string comment
        {
            get;
            set;
        }


        public virtual bool enabled
        {
            get;
            set;
        }

        [XmlArray("properties"), XmlArrayItem("property")]
        public Property[] properties
        {
            get;
            set;
        }

        [XmlArray("privileges"), XmlArrayItem("privilege")]
        public Privilege[] privileges
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return ToString().Equals(obj.ToString(), StringComparison.InvariantCultureIgnoreCase);
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        } 

        public override string ToString()
        {
            return String.Format(@"{0}\{1}", realm ?? "tempuri.org", userName ?? "");
        }

        #endregion

        public void OnDeserialization(object sender)
        {
           
        }

        [OnDeserialized]
        public void OnDeserialized(StreamingContext context)
        {
            // initialize unserialized fields etc.
        }

    }
}
