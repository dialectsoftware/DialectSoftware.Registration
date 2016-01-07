using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Specialized;
using System.Web;
using System.Web.Configuration;
using DialectSoftware.Registration.Data;

namespace DialectSoftware.Registration
{
    public class RoleProvider : System.Web.Security.RoleProvider, IRoleProvider
    {
        private string _connectionString;

        public override void AddUsersToRoles(string[] usernames, string[] roleNames)
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

        public override void CreateRole(string roleName)
        {
            throw new NotImplementedException();
        }

        public override bool DeleteRole(string roleName, bool throwOnPopulatedRole)
        {
            throw new NotImplementedException();
        }

        public override string[] FindUsersInRole(string roleName, string usernameToMatch)
        {
            return GetUsersInRole(roleName).Where(r => r == usernameToMatch).ToArray();
        }

        public override string[] GetAllRoles()
        {
            using (var Entities = Repository)
            {
                return Entities.Roles_v1_0.Select(r => r.roleName).ToArray();
            }
        }

        public override string[] GetRolesForUser(string username)
        {
            Guid applicationInstance = Guid.Parse(ApplicationName);
            using (var Entities = Repository)
            {
                return Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email == username)
                    .Select(m => m.Privileges_v1_0.Roles_v1_0.roleName)
                    .ToArray();
            }
        }

        public override string[] GetUsersInRole(string roleName)
        {
            Guid applicationInstance = Guid.Parse(ApplicationName);
            using (var Entities = Repository)
            {
                return Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Privileges_v1_0.Roles_v1_0.roleName == roleName)
                    .Select(m => m.Users_v1_0.email)
                    .ToArray();
            }
        }

        public override bool IsUserInRole(string username, string roleName)
        {
            return GetRolesForUser(username).Contains(roleName);
        }

        public override void RemoveUsersFromRoles(string[] usernames, string[] roleNames)
        {
            throw new NotImplementedException();
        }

        public override bool RoleExists(string roleName)
        {
            return GetAllRoles().Contains(roleName);
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
