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
    [XmlRoot("phone")]
    public class Phone
    {
        #region Primitive Properties

        public virtual int id
        {
            get;
            set;
        }

        public virtual string formattedPhoneNumber
        {
            get;
            set;
        }

        public virtual string countryCode
        {
            get;
            set;
        }

        public virtual string areaCode
        {
            get;
            set;
        }

        public virtual string phoneNumber
        {
            get;
            set;
        }

        public virtual string extension
        {
            get;
            set;
        }

        public virtual string countryName
        {
            get;
            set;
        }

        public virtual string isoCountryCode
        {
            get;
            set;
        }

        public int phoneType
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj is Phone && ((Phone)obj).ToString() == ToString();
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            var util = PhoneNumbers.PhoneNumberUtil.GetInstance();
            var number =  PhoneNumbers.PhoneNumberUtil.GetInstance().Parse(String.Format("+{0} {1} {2}", countryCode??"", areaCode??"", phoneNumber??""),isoCountryCode);
            return util.Format(number, PhoneNumbers.PhoneNumberFormat.INTERNATIONAL);
        }
        #endregion
    }
}
