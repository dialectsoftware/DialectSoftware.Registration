using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Configuration;
using System.Web;
using System.Collections.Specialized;
using System.Web.Security;
using System.Configuration.Provider;
using DialectSoftware.Mail;
using System.Xml.Linq;
using DialectSoftware.Security;
using DialectSoftware.Registration.Documents;


namespace DialectSoftware.Registration
{
    public class MembershipProvider:System.Web.Security.MembershipProvider, IMembershipProvider
    {
        private string _connectionString;

        public MembershipProvider()
        {
        
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

        public Guid ApplicationInstance
        {
            get
            {
                return Guid.Parse(ApplicationName);
            }
        }

        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            throw new NotImplementedException();
        }

        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            throw new NotImplementedException();
        }

        public MembershipUser CreateContact(string firstname, string lastname, string middlename, string email, string username, string password, string passwordQuestion, string passwordAnswer, bool isApproved, out System.Web.Security.MembershipCreateStatus status)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "firstname");
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "lastname");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
    
            int count;
            FindUsersByEmail(email, 0, 1, out count);
            if (count == 0)
                FindUsersByName(username, 0, 1, out count);
            else
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }

            if (count == 0)
            {
                System.Net.Mail.MailAddress address = new System.Net.Mail.MailAddress(email);
                Contact contact = null;
                using (var Entities = Repository)
                {
                    var contacts = Entities.ContactEmails_v1_0.Where(e => e.Email_v1_0.formattedEmailAddress == email);
                    if (contacts.Count() == 1)
                    {
                        status = MembershipCreateStatus.DuplicateEmail;
                        return null;
                    }
                    else
                    {
                        contact = new Contacts_v1_0
                        {
                            firstName = firstname,
                            lastName = lastname,
                            middleName = middlename,
                            givenName = firstname,
                            firstPartOfLastName = lastname.GetCharLeftPart(),
                            createDate = DateTime.Now
                        };
                        Email_v1_0 mail = new Email_v1_0
                        {
                            formattedEmailAddress = address.Address,
                            domainName = address.Host.Split('.')[0],
                            extension = address.Host.Split('.')[1],
                            localpart = address.User,
                            emailTypeId = 2 //TODO:Detect
                        };
                        ContactEmails_v1_0 contactMail = new ContactEmails_v1_0
                        {
                            Contacts_v1_0 = contact,
                            Email_v1_0 = mail
                        };

                        return CreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, contact, out status);
                    }
                }
            }
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }
              
        }

        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out System.Web.Security.MembershipCreateStatus status)
        {
            if (String.IsNullOrEmpty(username)) throw new ArgumentException("Value cannot be null or empty.", "userName");
            if (String.IsNullOrEmpty(password)) throw new ArgumentException("Value cannot be null or empty.", "password");
            if (String.IsNullOrEmpty(email)) throw new ArgumentException("Value cannot be null or empty.", "email");
           
            int count;
            FindUsersByName(username,0,1, out count);
            if (count == 0)
                FindUsersByEmail(email, 0, 1, out count);
            else
            {
                status = MembershipCreateStatus.DuplicateUserName;
                return null;
            }

            if (count == 0)
            {
                var application = Subscription.GetApplicationInformation();
                Contact contact = providerUserKey as Contact;
                if (contact == null)
                {
                    status = MembershipCreateStatus.UserRejected;
                    return null;
                }

                User user = new User
                {
                    email = email,
                    userName = username,
                    createDate = DateTime.Now,
                    isAnonymous = false,
                    isConfirmed = false,
                    isLockedOut = false,
                    lastActivityDate = DateTime.Now,
                    passwordFormat = ((int)PasswordFormat) + 1,
                    passwordSalt = Guid.NewGuid().ToString(),
                    password = password.GetMD5HashCode(),
                    passwordQuestion = passwordQuestion,
                    passwordAnswer = passwordAnswer,
                    enabled = true
                    
                 };

                Repository.Add(user);
                status = System.Web.Security.MembershipCreateStatus.Success;
                return new System.Web.Security.MembershipUser(this.Name, username, contact, email, passwordQuestion, user.comment, user.isConfirmed, user.isLockedOut, user.createDate, user.lastLoginDate, user.lastActivityDate, user.lastPasswordChangedDate, user.lastLockoutDate);
      
            }
            else
            {
                status = MembershipCreateStatus.DuplicateEmail;
                return null;
            }
        }

        public override bool DeleteUser(string username, bool deleteAllRelatedData)
        {
            throw new NotImplementedException();
        }

        public override bool EnablePasswordReset
        {
            get { return bool.Parse(Settings["enablePasswordReset"] ?? "True"); }
        }

        public override bool EnablePasswordRetrieval
        {
            get { return bool.Parse(Settings["enablePasswordRetrieval"] ?? "True"); }
        }

        public override System.Web.Security.MembershipUserCollection FindUsersByEmail(string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindMemberByEmail(ApplicationInstance, emailToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override System.Web.Security.MembershipUserCollection FindUsersByName(string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            return FindMemberByName(ApplicationInstance, usernameToMatch, pageIndex, pageSize, out totalRecords);
        }

        public override System.Web.Security.MembershipUserCollection GetAllUsers(int pageIndex, int pageSize, out int totalRecords)
        {
            string applicationName = HttpContext.Current == null ? ApplicationName : HttpContext.Current.Request.Params["applicationName"] ?? ApplicationName;
            return GetAllMembers(Guid.Parse(applicationName), pageIndex, pageSize, out totalRecords);
        }

        public override int GetNumberOfUsersOnline()
        {
            throw new NotImplementedException();
        }

        public override string GetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override System.Web.Security.MembershipUser GetUser(string username, bool userIsOnline)
        {
            return GetMember(ApplicationInstance, username, userIsOnline);
        }

        public override System.Web.Security.MembershipUser GetUser(object providerUserKey, bool userIsOnline)
        {
            throw new NotImplementedException();
        }

        public override string GetUserNameByEmail(string email)
        {
            int i;
            IEnumerable<MembershipUser> members = FindUsersByEmail(email,0,1,out i).Cast<MembershipUser>();
            if(i == 1)
                return members.Single().UserName;
            else
                throw new Exception("duplicate Email");
        }

        public override int MaxInvalidPasswordAttempts
        {
            get { return Int32.Parse(Settings["maxInvalidPasswordAttempts"] ?? "3"); }
        }

        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return Int32.Parse(Settings["minRequiredNonalphanumericCharacters"] ?? "0"); }
        }

        public override int MinRequiredPasswordLength
        {
            get { return Int32.Parse(Settings["minRequiredPasswordLength"] ?? "8"); }
        }

        public override int PasswordAttemptWindow
        {
            get { return Int32.Parse(Settings["passwordAttemptWindow"] ?? "30"); }
        }

        public override System.Web.Security.MembershipPasswordFormat PasswordFormat
        {
            get { return (MembershipPasswordFormat)Enum.Parse(typeof(MembershipPasswordFormat), Settings["passwordFormat"] ?? MembershipPasswordFormat.Hashed.ToString()); }
        }

        public override string PasswordStrengthRegularExpression
        {
            get { return @"(?=.{8,})(?=(.*\d){1,})(?=(.*\W){1,})"; }
        }

        public override bool RequiresQuestionAndAnswer
        {
            get { return bool.Parse(Settings["requiresQuestionAndAnswer"] ?? "False"); }
        }

        public override bool RequiresUniqueEmail
        {
            get { return bool.Parse(Settings["requiresUniqueEmail"] ?? "True"); }
        }

        public override string ResetPassword(string username, string answer)
        {
            throw new NotImplementedException();
        }

        public override bool UnlockUser(string userName)
        {
            try
            {
                Unlock(userName);
                return true;
            }
            catch
            {
                return false;
            }

        }

        public override void UpdateUser(System.Web.Security.MembershipUser user)
        {
            throw new NotImplementedException();
        }

        public override bool ValidateUser(string username, string password)
        {
            
            using (var Entities = Repository)
            {
                Guid applicationInstance = Guid.Parse(ApplicationName);
                var user = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m =>
                        {
                            switch (PasswordFormat)
                            {
                                case MembershipPasswordFormat.Encrypted:
                                    return m.Users_v1_0.email == username && !m.Users_v1_0.isLockedOut && m.Users_v1_0.password.Equals(Encryption.Encrypt(password), StringComparison.InvariantCultureIgnoreCase);
                                case MembershipPasswordFormat.Hashed:
                                    return m.Users_v1_0.email == username && !m.Users_v1_0.isLockedOut && m.Users_v1_0.password.Equals(password.GetMD5HashCode(), StringComparison.InvariantCultureIgnoreCase);
                                default:
                                    return m.Users_v1_0.email == username && !m.Users_v1_0.isLockedOut && m.Users_v1_0.password.Equals(password, StringComparison.InvariantCultureIgnoreCase);
                            }

                        })
                    .Select(m => m.Users_v1_0);
                return user.Count() == 1;
            }
        }

        public override void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            string connectionStringName = config["connectionStringName"];
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            //Entities = new RegistrationEntities(connectionString);
            Settings = config;

            //http://msdn.microsoft.com/en-us/library/6tc47t75.aspx
            MachineKeySection machineKey = WebConfigurationManager.GetSection("system.web/machineKey") as MachineKeySection;
            if (machineKey.ValidationKey.Contains("AutoGenerate"))
            {
                if (PasswordFormat != MembershipPasswordFormat.Clear)
                {
                    throw new ProviderException("Hashed or Encrypted passwords are not supported with auto-generated keys.");
                }
            }

            base.Initialize(name, config);
        }

        public virtual void Unlock(string email)
        {
            using (var Entities = Repository)
            {
                var user = Entities.Users_v1_0.Where(u => u.userName == System.Threading.Thread.CurrentPrincipal.Identity.Name).Single().contactId;
                var users = Entities.Users_v1_0.Where(u => u.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
                switch (users.Count())
                {
                    case 0:
                        throw new KeyNotFoundException(email);
                    case 1:
                        users.Single().isLockedOut = false;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public virtual void Confirm(string email)
        {
            using (var Entities = Repository)
            {
                var users = Entities.Users_v1_0.Where(u => u.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
                switch (users.Count())
                {
                    case 0:
                        throw new KeyNotFoundException(email);
                    case 1:
                        users.Single().isConfirmed = true;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public virtual void Enable(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));

                Membership_v1_0 member = null;
                switch (members.Count())
                {
                    case 0:
                        throw new KeyNotFoundException();
                    case 1:
                        member = members.Single();
                        member.enabled = true;
                        member.lastModifiedDate = DateTime.Now;
                        member.lastModifiedBy = authority;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }

        }

        public virtual void Disable(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));

                Membership_v1_0 member = null;
                switch (members.Count())
                {
                    case 0:
                        throw new KeyNotFoundException();
                    case 1:
                        member = members.Single();
                        member.enabled = false;
                        member.lastModifiedDate = DateTime.Now;
                        member.lastModifiedBy = authority;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public virtual void Allow(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));

                Membership_v1_0 member = null;
                switch (members.Count())
                {
                    case 0:
                        throw new KeyNotFoundException();
                    case 1:
                        member = members.Single();
                        member.privilegeId = 3;
                        member.isApproved = true;
                        member.approvedBy = authority;
                        member.approvedDate = DateTime.Now;
                        member.lastModifiedDate = member.approvedDate;
                        member.lastModifiedBy = authority;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public virtual void Deny(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var members = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));

                Membership_v1_0 member = null;
                switch (members.Count())
                {
                    case 0:
                        throw new KeyNotFoundException(email);
                    case 1:
                        member = members.Single();
                        member.privilegeId = 4;
                        member.isApproved = false;
                        member.lastModifiedDate = member.approvedDate;
                        member.lastModifiedBy = authority;
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception();
                }
            }
        }

        public MembershipUser GetUser(string username, string password, bool userIsOnline)
        {
            using (var Entities = Repository)
            {
                return
                     Entities
                    .Users_v1_0
                    .Where(m => m.email == username && !m.isLockedOut)
                    .AsEnumerable()
                    .Where(m =>
                    {
                        switch (PasswordFormat)
                        {
                            case MembershipPasswordFormat.Encrypted:
                                return m.email == username && !m.isLockedOut && m.password.Equals(Encryption.Encrypt(password), StringComparison.InvariantCultureIgnoreCase);
                            case MembershipPasswordFormat.Hashed:
                                return m.email == username && !m.isLockedOut && m.password.Equals(password.GetMD5HashCode(), StringComparison.InvariantCultureIgnoreCase);
                            default:
                                return m.email == username && !m.isLockedOut && m.password.Equals(password, StringComparison.InvariantCultureIgnoreCase);
                        }
                    }).Select(m => new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.userName, Subscription.GetApplicationRoles(m.email), m.email, m.passwordQuestion, m.comment, m.isConfirmed, m.isLockedOut, m.createDate, m.lastLoginDate ?? DateTime.MinValue, m.lastActivityDate ?? DateTime.MinValue, m.lastPasswordChangedDate ?? DateTime.MinValue, m.lastLockoutDate ?? DateTime.MinValue)
                    {
                        Email = m.email,
                        IsConfirmed = m.isConfirmed
                    })
                                .FirstOrDefault();


            }
        }

        public virtual string CreateConfirmationToken(string email, string memento = null)
        { 
            Guid applicationInstance = Guid.Parse(ApplicationName);
            using (var Entities = Repository)
            {
                var user = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email == email)
                    .Select(m => m.Users_v1_0)
                    .SingleOrDefault();
                if (user == null)
                    return null;
                else
                    return new ValidationToken(email) { Memento = memento };
            }
        }

        public virtual bool IsValidApplicationMember(Guid applicationInstance, string emailToMatch)
        {
            using (var Entities = Repository)
            {
                var users = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email.Equals(emailToMatch, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(m => m.Users_v1_0.email)
                    .Select(m =>
                    {
                        return new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.Users_v1_0.userName, m.Users_v1_0, m.Users_v1_0.email, m.Users_v1_0.passwordQuestion, m.Users_v1_0.comment, m.Users_v1_0.isConfirmed, m.Users_v1_0.isLockedOut, m.Users_v1_0.createDate, m.Users_v1_0.lastLoginDate ?? DateTime.MinValue, m.Users_v1_0.lastActivityDate ?? DateTime.MinValue, m.Users_v1_0.lastPasswordChangedDate ?? DateTime.MinValue, m.Users_v1_0.lastLockoutDate ?? DateTime.MinValue)
                        {
                            Email = m.Users_v1_0.email,
                            IsApproved = m.isApproved,
                            IsConfirmed = m.Users_v1_0.isConfirmed
                        };

                    }).ToList();
                return users.Count() == 1 && users.Single().IsConfirmed && users.Single().IsApproved && !users.Single().IsLockedOut;
            }
        }

        public System.Web.Security.MembershipUser GetMember(Guid applicationInstance, string username, bool userIsOnline)
        {
            using (var Entities = Repository)
            {
                var user = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email.Equals(username, StringComparison.InvariantCultureIgnoreCase))
                    .Select(m =>
                    {
                        var roles = Roles.Provider.GetRolesForUser(username);
                        return new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.Users_v1_0.userName, roles, m.Users_v1_0.email, m.Users_v1_0.passwordQuestion, m.Users_v1_0.comment, m.Users_v1_0.isConfirmed, m.Users_v1_0.isLockedOut, m.Users_v1_0.createDate, m.Users_v1_0.lastLoginDate ?? DateTime.MinValue, m.Users_v1_0.lastActivityDate ?? DateTime.MinValue, m.Users_v1_0.lastPasswordChangedDate ?? DateTime.MinValue, m.Users_v1_0.lastLockoutDate ?? DateTime.MinValue)
                        {
                            Email = m.Users_v1_0.email,
                            IsApproved = m.isApproved,
                            IsConfirmed = m.Users_v1_0.isConfirmed
                        };
                    })
                    .FirstOrDefault();

                return user;
            }

        }

        public virtual System.Web.Security.MembershipUserCollection FindMemberByEmail(Guid applicationInstance, string emailToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                totalRecords = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email.Equals(emailToMatch, StringComparison.InvariantCultureIgnoreCase)).Count();
                var users = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email.Equals(emailToMatch, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(m => m.Users_v1_0.email)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m =>
                    {
                        var role = Subscription.GetApplicationRoles(m.Users_v1_0.email).Single(r=>r.ApplicationUID == applicationInstance);
                        return new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.Users_v1_0.userName, role, m.Users_v1_0.email, m.Users_v1_0.passwordQuestion, m.Users_v1_0.comment, m.Users_v1_0.isConfirmed, m.Users_v1_0.isLockedOut, m.Users_v1_0.createDate, m.Users_v1_0.lastLoginDate ?? DateTime.MinValue, m.Users_v1_0.lastActivityDate ?? DateTime.MinValue, m.Users_v1_0.lastPasswordChangedDate ?? DateTime.MinValue, m.Users_v1_0.lastLockoutDate ?? DateTime.MinValue)
                        {
                            Email = m.Users_v1_0.email,
                            IsApproved = m.isApproved,
                            IsConfirmed = m.Users_v1_0.isConfirmed
                        };

                    }).ToList();
                MembershipUserCollection all = new MembershipUserCollection();
                users.ForEach(u =>
                {
                    all.Add(u);
                });
                return all;
            }
        }

        public virtual System.Web.Security.MembershipUserCollection FindMemberByName(Guid applicationInstance, string usernameToMatch, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                totalRecords = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.userName.Equals(usernameToMatch, StringComparison.InvariantCultureIgnoreCase)).Count();
                var users = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.userName.Equals(usernameToMatch, StringComparison.InvariantCultureIgnoreCase))
                    .OrderBy(m => m.Users_v1_0.email)
                    .Skip((pageIndex - 1) * pageSize)
                    .Take(pageSize)
                    .Select(m =>
                    {
                        var role = Subscription.GetApplicationRoles(m.Users_v1_0.email).Single(r => r.ApplicationUID == applicationInstance);
                        return new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.Users_v1_0.userName, role, m.Users_v1_0.email, m.Users_v1_0.passwordQuestion, m.Users_v1_0.comment, m.Users_v1_0.isConfirmed, m.Users_v1_0.isLockedOut, m.Users_v1_0.createDate, m.Users_v1_0.lastLoginDate ?? DateTime.MinValue, m.Users_v1_0.lastActivityDate ?? DateTime.MinValue, m.Users_v1_0.lastPasswordChangedDate ?? DateTime.MinValue, m.Users_v1_0.lastLockoutDate ?? DateTime.MinValue)
                        {
                            Email = m.Users_v1_0.email,
                            IsApproved = m.isApproved,
                            IsConfirmed = m.Users_v1_0.isConfirmed
                        };

                    }).ToList();
                MembershipUserCollection all = new MembershipUserCollection();
                users.ForEach(u =>
                {
                    all.Add(u);
                });
                return all;
            }
        }

        public virtual System.Web.Security.MembershipUserCollection GetAllMembers(Guid applicationInstance, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {
                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                totalRecords = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Select(m => m).Count();
                var users = Entities.
                     ApplicationInstance_v1_0
                     .Single(i => i.instanceUID == applicationInstance)
                     .Membership_v1_0
                     .Select(m => m)
                     .OrderBy(m => m.Users_v1_0.email)
                     .Skip((pageIndex - 1) * pageSize)
                     .Take(pageSize)
                     .Select(m =>
                     {
                         var role = Subscription.GetApplicationRoles(m.Users_v1_0.email).Single(r => r.ApplicationUID == applicationInstance);
                         return new DialectSoftware.Registration.Security.MembershipUser(this.Name, m.Users_v1_0.userName, role, m.Users_v1_0.email, m.Users_v1_0.passwordQuestion, m.Users_v1_0.comment, m.Users_v1_0.isConfirmed, m.Users_v1_0.isLockedOut, m.Users_v1_0.createDate, m.Users_v1_0.lastLoginDate ?? DateTime.MinValue, m.Users_v1_0.lastActivityDate ?? DateTime.MinValue, m.Users_v1_0.lastPasswordChangedDate ?? DateTime.MinValue, m.Users_v1_0.lastLockoutDate ?? DateTime.MinValue)
                         {
                             Email = m.Users_v1_0.email,
                             IsApproved = m.isApproved,
                             IsConfirmed = m.Users_v1_0.isConfirmed
                         };
                     }).ToList();
                MembershipUserCollection all = new MembershipUserCollection();
                users.ForEach(u =>
                {
                    all.Add(u);
                });
                return all;
            }
        }

        public virtual void ValidateConfirmationToken(string email, ValidationToken token, out ValidationTokenStatus status)
        {
            using (var Entities = Repository)
            {
                Guid applicationInstance = Guid.Parse(ApplicationName);
                var user = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0.Where(m => m.Users_v1_0.email == email)
                    .Select(m => m.Users_v1_0).SingleOrDefault();
                if (user == null)
                {
                    status = ValidationTokenStatus.UserNotFound;
                }
                else
                {
                    try
                    {
                        status = token.Validate(email);
                    }
                    catch
                    {
                        status = ValidationTokenStatus.InvalidToken;
                    }

                }
            }
        }

        public String EULA
        {
            get
            {
                System.Configuration.Configuration config = null;
                if (System.Web.HttpContext.Current == null)
                    config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                else
                    config = WebConfigurationManager.OpenWebConfiguration("/"); 

                //var smtp = config.GetSectionGroup(@"system.net/mailSettings").Sections["smtp"] as System.Net.Configuration.SmtpSection;
                //XComment comment =  XDocument.Parse(smtp.SectionInformation.GetRawXml()).Document.Root.Element(XName.Get("network")).DescendantNodes().SingleOrDefault() as XComment;

                var comment = XDocument.Parse(config.GetSectionGroup(@"system.web")
                    .Sections["membership"]
                    .SectionInformation.GetRawXml())
                    .Document
                    .Root
                    .Element(XName.Get("providers"))
                    .Elements(XName.Get("add"))
                    .Where(e => e.Attribute(XName.Get("name")).Value.Equals(Name))
                    .Single()
                    .DescendantNodes()
                    .SingleOrDefault() as XComment;

                if (comment == null)
                    return null;
                else
                    return comment.Value;
            }
        }

        private int GetCurrentAuthority(Guid applicationInstance)
        {
            return 0;

        }

        protected NameValueCollection Settings
        {
            get;
            private set;
        }

        protected Users Repository
        {
            get
            {
                var builder = new System.Data.Common.DbConnectionStringBuilder();
                builder.ConnectionString = _connectionString;
                object server = builder["server"]??"http://127.0.0.1:5984";
                object username = builder["username"];
                object password = builder["password"];
                SharpCouch.DB couch;
                if (username != null && password != null)
                    couch = new SharpCouch.DB(server.ToString(), username.ToString(), password.ToString());
                else
                    couch = new SharpCouch.DB(server.ToString());
                return Users.Connect(couch,builder["db"].ToString());
            }
        }

    }
}
