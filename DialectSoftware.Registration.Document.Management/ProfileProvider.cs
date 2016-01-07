using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration;
using System.Web;
using System.Collections.Specialized;
using DialectSoftware.Registration.Data;

namespace DialectSoftware.Registration
{
    public class ProfileProvider:System.Web.Profile.ProfileProvider, IProfileProvider
    {
        private string _connectionString;

        public ProfileProvider()
        {
            //ProfileSection profileSection = (ProfileSection)WebConfigurationManager.GetSection("system.web/profiles");
            //string defaultProvider = null;
            //for (int i = 0; i < profileSection.Providers.Count; i++)
            //{
            //    string type = typeof(ProfileProvider).Namespace + "." + typeof(ProfileProvider).Name;
            //    if (profileSection.Providers[i].Type.StartsWith(type))
            //    {
            //        defaultProvider = profileSection.Providers[i].Name;
            //    }
            //}
            //Settings = profileSection.Providers[defaultProvider];
            //string connectionStringName = Settings.Parameters["connectionStringName"];
            //string connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            //Entities = new RegistrationEntities(connectionString);
        }

        public override int DeleteInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(string[] usernames)
        {
            throw new NotImplementedException();
        }

        public override int DeleteProfiles(System.Web.Profile.ProfileInfoCollection profiles)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Profile.ProfileInfoCollection FindInactiveProfilesByUserName(System.Web.Profile.ProfileAuthenticationOption authenticationOption, string usernameToMatch, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            pageIndex = pageIndex < 1 ? 0 : pageIndex - 1;
            throw new NotImplementedException();
        }

        public override System.Web.Profile.ProfileInfoCollection FindProfilesByUserName(System.Web.Profile.ProfileAuthenticationOption authenticationOption, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                Guid applicationInstance = Guid.Parse(ApplicationName);
                System.Web.Profile.ProfileInfoCollection profiles = new System.Web.Profile.ProfileInfoCollection();
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email == usernameToMatch && m.Users_v1_0.isAnonymous == (authenticationOption == System.Web.Profile.ProfileAuthenticationOption.Anonymous || authenticationOption == System.Web.Profile.ProfileAuthenticationOption.All));

                totalRecords = members.Count();

                members.Skip((pageIndex - 1) * pageSize)
                    .ToList().ForEach(m =>
                    {
                        profiles.Add(new System.Web.Profile.ProfileInfo(m.Users_v1_0.email, m.Users_v1_0.isAnonymous, m.Users_v1_0.lastActivityDate.Value, m.Profile_v1_0.Max(p => p.LastUpdatedDate), m.Profile_v1_0.Count()));
                    });
                return profiles;
            }
        }

        public override System.Web.Profile.ProfileInfoCollection GetAllInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate, int pageIndex, int pageSize, out int totalRecords)
        {
            pageIndex = pageIndex < 1 ? 1 : pageIndex;
            throw new NotImplementedException();
        }

        public override System.Web.Profile.ProfileInfoCollection GetAllProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                Guid applicationInstance = Guid.Parse(ApplicationName);
                System.Web.Profile.ProfileInfoCollection profiles = new System.Web.Profile.ProfileInfoCollection();
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0;

                totalRecords = members.Count();

                members.Skip((pageIndex - 1) * pageSize)
                    .ToList().ForEach(m =>
                    {
                        profiles.Add(new System.Web.Profile.ProfileInfo(m.Users_v1_0.email, m.Users_v1_0.isAnonymous, m.Users_v1_0.lastActivityDate.Value, m.Profile_v1_0.Max(p => p.LastUpdatedDate), m.Profile_v1_0.Count()));
                    });
                return profiles;
            }
        }

        public override int GetNumberOfInactiveProfiles(System.Web.Profile.ProfileAuthenticationOption authenticationOption, DateTime userInactiveSinceDate)
        {
            throw new NotImplementedException();
        }

        public override string ApplicationName
        {
            get
            {
                return Settings["applicationName"];
            }
            set
            {
                Settings["applicationName"] = value;
            }
        }

        public override System.Configuration.SettingsPropertyValueCollection GetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyCollection collection)
        {
            throw new NotImplementedException();
        }

        public override void SetPropertyValues(System.Configuration.SettingsContext context, System.Configuration.SettingsPropertyValueCollection collection)
        {
            throw new NotImplementedException();
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            string connectionStringName = config["connectionStringName"];
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            //Entities = new RegistrationEntities(connectionString);
            Settings = config;
            base.Initialize(name, config);
        }

        protected NameValueCollection Settings
        {
            get;
            private set;
        }

        protected RegistrationEntities Repository
        {
            get
            {
                var repository = new RegistrationEntities(_connectionString);
                repository.SetMergeOptions(System.Data.Objects.MergeOption.NoTracking);
                return repository;
            }
        }

    }
}
