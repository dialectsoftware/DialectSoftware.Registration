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
    [XmlRoot("contact")]
    public class Contact
    {
        public Contact()
            : this(new Guid())
        { 
        
        }

        public Contact(Guid uuid)
        {
            contactUID = uuid == Guid.Empty ? Guid.NewGuid() : uuid;
            phoneNumbers = new List<Phone>();
            onlineAccounts = new List<OnlineAccount>();
            addresses = new List<Address>();
            emailAddresses = new List<Email>(); 
            properties = new List<Property>();
            users = new List<User>();
        }

        #region Primitive Properties

        public string _id
        {
            get { return contactUID.ToString();  }
        }

        public string _rev
        {
            get;
            set;
        }

        public string type
        {
            get { return "Contact"; }
        }

        public Guid contactUID
        {
            get;
            set;
        }

        public virtual DateTime dateofBirth
        {
            get;
            set;
        }

        public virtual string formOfAddress
        {
            get;
            set;
        }

        public virtual string givenName
        {
            get;
            set;
        }

        public virtual string firstName
        {
            get;
            set;
        }

        public virtual string middleName
        {
            get;
            set;
        }

        public virtual string preferredName
        {
            get;
            set;
        }

        public virtual string surnamePrefix
        {
            get;
            set;
        }

        public virtual string surname
        {
            get;
            set;
        }

        public virtual string firstPartOfLastName
        {
            get;
            set;
        }

        public virtual string lastName
        {
            get;
            set;
        }

        public virtual string nameSuffixOrGeneration
        {
            get;
            set;
        }

        public virtual string qualification
        {
            get;
            set;
        }

        public virtual string precedingQualification
        {
            get;
            set;
        }

        public virtual string intermediateQualification
        {
            get;
            set;
        }

        public virtual string succeedingQualification
        {
            get;
            set;
        }

        public virtual System.DateTime createDate
        {
            get;
            set;
        }

        [XmlArray("properties"), XmlArrayItem("property")]
        public List<Property> properties
        {
            get;
            set;
        }

        [XmlArray("addresses"), XmlArrayItem("address")]
        public List<Address> addresses
        {
            get;
            set;
        }

        [XmlArray("phoneNumbers"), XmlArrayItem("phone")]
        public List<Phone> phoneNumbers
        {
            get;
            set;
        }

        [XmlArray("emailAddresses"), XmlArrayItem("email")]
        public List<Email> emailAddresses
        {
            get;
            set;
        }

        [XmlArray("onlineAccounts"), XmlArrayItem("account")]
        public List<OnlineAccount> onlineAccounts
        {
            get;
            set;
        }

        [XmlArray("users"), XmlArrayItem("user")]
        public List<User> users
        {
            get;
            set;
        }
     
        #endregion

        public override int GetHashCode()
        {
            return contactUID.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var contact = obj as Contact; 
            return contact != null && contact.contactUID == contactUID;
        }
    }
}
