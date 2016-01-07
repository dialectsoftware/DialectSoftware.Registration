using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml.Serialization;

namespace DialectSoftware.Registration
{
    //[Serializable]
    [XmlRoot("address")]
    public class Address
    {
        #region Primitive Properties

        public virtual string addressLine1
        {
            get;
            set;
        }

        public virtual string addressLine2
        {
            get;
            set;
        }

        public virtual string addressLine3
        {
            get;
            set;
        }

        public virtual string additionalAddressLine1
        {
            get;
            set;
        }

        public virtual string additionalAddressLine2
        {
            get;
            set;
        }

        public virtual string countyOrParishName
        {
            get;
            set;
        }

        public virtual string cityStateZipLine
        {
            get;
            set;
        }

        public virtual string cityOrTownName
        {
            get;
            set;
        }

        public virtual string stateProvinceRegion
        {
            get;
            set;
        }

        public virtual string stateProvinceAbbreviation
        {
            get;
            set;
        }

        public virtual string postcode
        {
            get;
            set;
        }

        public virtual string zipCode
        {
            get;
            set;
        }

        public virtual string zipPlusFourAddendum
        {
            get;
            set;
        }

        public virtual string zipCodeDeliveryPointAddOn
        {
            get;
            set;
        }

        public virtual string formattedPostalCode
        {
            get;
            set;
        }

        public virtual string countryName
        {
            get;
            set;
        }

        public virtual string countryIntlLanguage
        {
            get;
            set;
        }

        public virtual string countryIntlLanguageCode
        {
            get;
            set;
        }

        public virtual string countryIntlLanguageName
        {
            get;
            set;
        }

        public virtual string isoCountryCode
        {
            get;
            set;
        }

        public virtual int addressType
        {
            get;
            set;
        }

        public override bool Equals(object obj)
        {
            return obj != null && ToString().Equals(obj.ToString());
        }

        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        public override string ToString()
        {
            //all adresses reduced to a US approximation
            StringBuilder sb = new StringBuilder();
            sb.AppendOnly(addressLine1).AppendOnly(addressLine2).AppendOnly(addressLine3).AppendLine(String.Format("{0}, {1} {2}", cityOrTownName ?? "", stateProvinceAbbreviation ?? "", zipCode ?? ""));
            return sb.ToString();
        }
        #endregion
    }
}
