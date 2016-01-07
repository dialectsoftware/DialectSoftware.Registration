using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Configuration;
using System.Collections.Specialized;
using System.Web;
using DialectSoftware.Mail;
using DialectSoftware.Security;
using DialectSoftware.Text;
using System.Web.Security;
using DialectSoftware.Registration.Data;

namespace DialectSoftware.Registration
{
    public class SubscriptionProvider : ISubscriptionProvider
    {
        private string _connectionString;

        public string ApplicationName
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

        public virtual Contact GetContactInformation(string email)
        {
            return GetContactInformation(ApplicationInstance, email);
        }

        public virtual Contact GetContactInformation(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var contact = Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .Membership_v1_0
                    .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                    .Select(m => new Contact()
                    {
                        FirstName = m.Users_v1_0.Contacts_v1_0.firstName,
                        LastName = m.Users_v1_0.Contacts_v1_0.lastName,
                        Addresses = m.Users_v1_0.Contacts_v1_0.ContactAddress_v1_0.Select(a => new ContactAddress()
                        {
                            Type = a.AddressTypes_v1_0.addessType,
                            Address1 = a.Address_v1_0.addressLine1 ?? "",
                            Address2 = a.Address_v1_0.addressLine2 ?? "",
                            City = a.Address_v1_0.cityOrTownName ?? "",
                            State = a.Address_v1_0.stateProvinceRegion ?? "",
                            Zip = a.Address_v1_0.zipCode,
                            Country = a.Address_v1_0.countryName ?? ""
                        }).ToList(),
                        EmailAccounts = m.Users_v1_0.Contacts_v1_0.ContactEmails_v1_0.Select(e => new ContactEmail()
                        {
                            Type = e.Email_v1_0.EmailTypes_v1_0.emailType,
                            Email = e.Email_v1_0.formattedEmailAddress
                        }).ToList(),
                        PhoneNumbers = m.Users_v1_0.Contacts_v1_0.ContactPhone_v1_0.Select(p => new ContactPhone()
                        {
                            Type = p.PhoneTypes_v1_0.phoneType,
                            Phone = p.Phone_v1_0.formattedPhoneNumber
                        }).ToList(),
                        OnlineAccounts = m.Users_v1_0.Contacts_v1_0.ContactOnlineAccounts_v1_0.Select(o => new ContactOnline()
                        {
                            Type = o.OnlineAccounts_v1_0.OnlineAccountTypes_v1_0.service,
                            Account = o.OnlineAccounts_v1_0.username
                        }).ToList(),
                    }).SingleOrDefault();

                return contact;
            }
        }

        public virtual IEnumerable<ApplicationRole> GetApplicationRoles(string email)
        {
            using (var Entities = Repository)
            {
                var appRoles = Entities.Membership_v1_0.Where(m => m.Users_v1_0.email.Equals(email)).Select(m => new ApplicationRole()
                                                                                                            {
                                                                                                                ApplicationName = m.ApplicationInstance_v1_0.instanceName,
                                                                                                                ApplicationUID = m.ApplicationInstance_v1_0.instanceUID,
                                                                                                                Company = m.ApplicationInstance_v1_0.Business_v1_0.firmName,
                                                                                                                Alias = m.Users_v1_0.userName,
                                                                                                                Level = m.Privileges_v1_0.id,
                                                                                                                Role = m.Privileges_v1_0.Roles_v1_0.roleName,
                                                                                                                Priviledges = m.Privileges_v1_0.Rights_v1_0.name

                                                                                                            }).ToArray();
                return appRoles.Select(r =>
                {
                    r.IsDefault = r.ApplicationUID.Equals(Guid.Parse(ApplicationName));
                    return r;
                }).ToList();
            }

        }

        public virtual  Application GetApplicationInformation()
        {
            return GetApplicationInformation(Guid.Parse(ApplicationName));
        }

        public virtual  Application GetApplicationInformation(Guid applicationInstance)
        {
            using (var Entities = Repository)
            {
                return Entities.
                    ApplicationInstance_v1_0
                    .Where(i => i.instanceUID == applicationInstance)
                    .Select(app=> new Application
                        {
                            ApplicationInstance = app.instanceUID,
                            ApplicationDisplayName = app.instanceName,
                            ApplicationType = app.ApplicationInstanceType_v1_0.instanceType
                        }).Single();
            }
        }

        public virtual IEnumerable<MachineKey> GetMachineKeys(string email)
        {
            return GetMachineKeys(ApplicationInstance, email);
        }

        public virtual void DownloadMachineKeyRequest(string messageId)
        {
            try
            {
                DialectSoftware.Mail.IMailClient client = new GmailClient();
                var message = client.DownLoad(messageId);
            }
            catch
            {
                
            }
            finally
            {

            }
        }

        public virtual IEnumerable<MachineKey> DownloadMachineKeys()
        {
            try
            {
                DialectSoftware.Mail.IMailClient client = new GmailClient();
                return client.Mail.Where(
                        m => TextParser.Parse((string)m.Subject, new EMailToken()).Where(t => t.Name == "MACHINEID").Count() == 1
                          && TextParser.Parse((string)m.Subject, new EMailToken()).Where(t => t.Name == "APPLICATIONID").SingleOrDefault() != null
                    )
                    .Select(m =>
                    {
                        string title = m.Subject;
                        string email = m.From.Address;
                        string name = m.From.DisplayName;
                        string firstname = String.Empty;
                        string lastname = String.Empty;
                        string id = m.Uid;
                        string[] names = new []{firstname, lastname};
                        if (name.IndexOf(",") > 0)
                        {
                            names = name.Split(',');
                            Array.Reverse(names);
                        }
                        else
                        {
                            names = name.Split(' ');
                        }

                        firstname = names[0];
                        if(names.Length > 1)
                        {
                            lastname = names[1];     
                        }

                        string machineId = null;
                        string instanceId = null;
                        var machine = TextParser.Parse(title, new EMailToken()).Where(t=>t.Name == "MACHINEID").SingleOrDefault();
                        var instance = TextParser.Parse(title, new EMailToken()).Where(t => t.Name == "APPLICATIONID").SingleOrDefault();
                        if(machine != null)
                        {
                            machineId = machine.Items.Single();
                            title = title.Remove(machine.Items.Single().StartIndex, machine.Items.Single().Length);
                        }
                        if (instance != null)
                        {
                            instanceId = instance.Items.Single();
                            title = title.Remove(instance.Items.Single().StartIndex, instance.Items.Single().Length);
                        }
                        DateTime issued = m.Date; // DateTime.Parse(m.Date);
                        return new MachineKey
                        {
                            Key = machineId,
                            IsAuthorized = false,
                            Description = m.Subject,
                            CreateDate = issued,
                            Tag = id,
                            Contact = new Contact
                            {
                                FirstName = firstname,
                                LastName = lastname,
                            }
                        };
                    }).ToList();

            }
            catch
            {
                return null;
            }
            finally
            {

            }
        }

        public virtual IEnumerable<MachineKey> DownloadMachineKeys(Guid applicationInstance)
        {
            try
            {
                IMailClient client = new GmailClient();
                return client.Mail.Where(
                        m => TextParser.Parse((string)m.Subject, new EMailToken()).Where(t => t.Name == "MACHINEID").Count() == 1
                          && TextParser.Parse((string)m.Subject, new EMailToken()).Where(t => t.Name == "APPLICATIONID").SingleOrDefault() != null
                          && Guid.Parse(TextParser.Parse((string)m.Subject, new EMailToken()).Where(t => t.Name == "APPLICATIONID").SingleOrDefault().Items.Single()) == applicationInstance
                    )
                    .Select(m =>
                    {
                        try
                        {
                            string title = m.Subject;
                            string email;
                            string name;
                            //TODO:Fix in MailMessage class
                            if (m.From == null)
                            {
                                var header = ((AE.Net.Mail.MailMessage)m).Headers["From"].RawValue;
                                email = TextParser.Parse(header, new EMailToken()).Where(t => t.Name == "EMAIL").FirstOrDefault().Items.FirstOrDefault();
                                name = header.Substring(0, header.IndexOf(email) - 1).Trim(new char[] { ' ', '"' });
                            }
                            else
                            {
                                email = m.From.Address;
                                name = m.From.DisplayName;
                            }
                            string firstname = String.Empty;
                            string lastname = String.Empty;
                            string id = m.Uid;
                            string[] names = new[] { firstname, lastname };
                            if (name.IndexOf(",") > 0)
                            {
                                names = name.Split(',');
                                Array.Reverse(names);
                            }
                            else
                            {
                                names = name.Split(' ');
                            }

                            firstname = names[0];
                            if (names.Length > 1)
                            {
                                lastname = names[1];
                            }

                            string machineId = null;
                            string instanceId = null;
                            var machine = TextParser.Parse(title, new EMailToken()).Where(t => t.Name == "MACHINEID").SingleOrDefault();
                            var instance = TextParser.Parse(title, new EMailToken()).Where(t => t.Name == "APPLICATIONID").SingleOrDefault();
                            if (machine != null)
                            {
                                machineId = machine.Items.Single();
                                title = title.Remove(machine.Items.Single().StartIndex, machine.Items.Single().Length);
                            }
                            if (instance != null)
                            {
                                instanceId = instance.Items.Single();
                                title = title.Remove(instance.Items.Single().StartIndex, instance.Items.Single().Length);
                            }
                            DateTime issued = m.Date;
                            return new MachineKey
                            {
                                Email = email,
                                CreateDate = issued,
                                IsAuthorized = false,
                                Key = machineId.Trim('\\'),
                                ApplicationInstance = Guid.Parse(instanceId),
                                Description = title,
                                Tag = id,
                                Contact = new Contact
                                {
                                    FirstName = firstname,
                                    LastName = lastname,
                                }
                            };
                        }
                        catch
                        {
                            return null;
                        }
                    }).Where(m=>m!=null).ToList();

            }
            catch
            {
                return null;
            }
            finally
            {

            }
        }

        public virtual IEnumerable<MachineKey> GetMachineKeys(Guid applicationInstance, string email)
        {
            using (var Entities = Repository)
            {
                var member = Entities.
                   ApplicationInstance_v1_0
                   .Single(i => i.instanceUID == applicationInstance)
                   .Membership_v1_0
                   .Where(m => m.Users_v1_0.email.Equals(email, StringComparison.InvariantCultureIgnoreCase))
                   .DefaultIfEmpty(new Membership_v1_0() { Users_v1_0 = new Users_v1_0() { Contacts_v1_0 = new Contacts_v1_0() } })
                   .SingleOrDefault();

                return Entities.
                    ApplicationInstance_v1_0
                    .Single(i => i.instanceUID == applicationInstance)
                    .ApplicationInstanceMachineKeys_v1_0.Where(k => k.MachineKeys_v1_0.Contacts_v1_0.id == member.Users_v1_0.Contacts_v1_0.id)
                    .Select(k => new MachineKey()
                    {
                        Key = k.MachineKeys_v1_0.machineKey,
                        Description = k.MachineKeys_v1_0.machineDescription,
                        CreateDate = k.createDate,
                        IsAuthorized = k.authorized,
                        AuthorizedDate = k.authorizedDate.Value,
                        Email = member.Users_v1_0.email
                    }).ToList();
            }
        }

        public virtual IEnumerable<MachineKey> GetMachineKeysByApplication(Guid applicationInstance)
        {
            using (var Entities = Repository)
            {
                var app = Entities
                    .ApplicationInstance_v1_0
                   .Single(i => i.instanceUID == applicationInstance);

                var keys = app.ApplicationInstanceMachineKeys_v1_0
                            .Select(k => new MachineKey()
                            {
                                Key = k.MachineKeys_v1_0.machineKey,
                                Description = k.MachineKeys_v1_0.machineDescription,
                                CreateDate = k.createDate,
                                IsAuthorized = k.authorized,
                                AuthorizedDate = k.authorizedDate.Value,
                                Email = app.Membership_v1_0.Single(m => m.Users_v1_0.Contacts_v1_0 == k.MachineKeys_v1_0.Contacts_v1_0).Users_v1_0.email,
                                Contact = new Contact()
                                {
                                    FirstName = k.MachineKeys_v1_0.Contacts_v1_0.firstName,
                                    LastName = k.MachineKeys_v1_0.Contacts_v1_0.lastName,
                                    Addresses = k.MachineKeys_v1_0.Contacts_v1_0.ContactAddress_v1_0.Select(a => new ContactAddress()
                                    {
                                        Type = a.AddressTypes_v1_0.addessType,
                                        Address1 = a.Address_v1_0.addressLine1 ?? "",
                                        Address2 = a.Address_v1_0.addressLine2 ?? "",
                                        City = a.Address_v1_0.cityOrTownName ?? "",
                                        State = a.Address_v1_0.stateProvinceRegion ?? "",
                                        Zip = a.Address_v1_0.zipCode,
                                        Country = a.Address_v1_0.countryName ?? ""
                                    }).ToList(),
                                    EmailAccounts = k.MachineKeys_v1_0.Contacts_v1_0.ContactEmails_v1_0.Select(e => new ContactEmail()
                                    {
                                        Type = e.Email_v1_0.EmailTypes_v1_0.emailType,
                                        Email = e.Email_v1_0.formattedEmailAddress
                                    }).ToList(),
                                    PhoneNumbers = k.MachineKeys_v1_0.Contacts_v1_0.ContactPhone_v1_0.Select(p => new ContactPhone()
                                    {
                                        Type = p.PhoneTypes_v1_0.phoneType,
                                        Phone = p.Phone_v1_0.formattedPhoneNumber
                                    }).ToList(),
                                    OnlineAccounts = k.MachineKeys_v1_0.Contacts_v1_0.ContactOnlineAccounts_v1_0.Select(o => new ContactOnline()
                                    {
                                        Type = o.OnlineAccounts_v1_0.OnlineAccountTypes_v1_0.service,
                                        Account = o.OnlineAccounts_v1_0.username
                                    }).ToList(),
                                }
                            }).ToList();
                return keys;
            }
        }

        public virtual IEnumerable<MachineKey> GetMachineKeysByApplication(Guid applicationInstance, int pageIndex, int pageSize, out int totalRecords)
        {
            using (var Entities = Repository)
            {

                pageIndex = pageIndex < 1 ? 1 : pageIndex;
                var app = Entities
                    .ApplicationInstance_v1_0
                   .Single(i => i.instanceUID == applicationInstance);
                totalRecords = app.ApplicationInstanceMachineKeys_v1_0.Count();
                var keys = app.ApplicationInstanceMachineKeys_v1_0
                            .Skip((pageIndex - 1) * pageSize)
                            .Take(pageSize)
                            .Select(k => new MachineKey
                            {
                                Key = k.MachineKeys_v1_0.machineKey,
                                Description = k.MachineKeys_v1_0.machineDescription,
                                CreateDate = k.createDate,
                                IsAuthorized = k.authorized,
                                AuthorizedDate = k.authorizedDate.Value,
                                Email = app.Membership_v1_0.Single(m => m.Users_v1_0.Contacts_v1_0 == k.MachineKeys_v1_0.Contacts_v1_0).Users_v1_0.email,
                                Contact = new Contact()
                                {
                                    FirstName = k.MachineKeys_v1_0.Contacts_v1_0.firstName,
                                    LastName = k.MachineKeys_v1_0.Contacts_v1_0.lastName,
                                    Addresses = k.MachineKeys_v1_0.Contacts_v1_0.ContactAddress_v1_0.Select(a => new ContactAddress()
                                    {
                                        Type = a.AddressTypes_v1_0.addessType,
                                        Address1 = a.Address_v1_0.addressLine1 ?? "",
                                        Address2 = a.Address_v1_0.addressLine2 ?? "",
                                        City = a.Address_v1_0.cityOrTownName ?? "",
                                        State = a.Address_v1_0.stateProvinceRegion ?? "",
                                        Zip = a.Address_v1_0.zipCode,
                                        Country = a.Address_v1_0.countryName ?? ""
                                    }).ToList(),
                                    EmailAccounts = k.MachineKeys_v1_0.Contacts_v1_0.ContactEmails_v1_0.Select(e => new ContactEmail()
                                    {
                                        Type = e.Email_v1_0.EmailTypes_v1_0.emailType,
                                        Email = e.Email_v1_0.formattedEmailAddress
                                    }).ToList(),
                                    PhoneNumbers = k.MachineKeys_v1_0.Contacts_v1_0.ContactPhone_v1_0.Select(p => new ContactPhone()
                                    {
                                        Type = p.PhoneTypes_v1_0.phoneType,
                                        Phone = p.Phone_v1_0.formattedPhoneNumber
                                    }).ToList(),
                                    OnlineAccounts = k.MachineKeys_v1_0.Contacts_v1_0.ContactOnlineAccounts_v1_0.Select(o => new ContactOnline()
                                    {
                                        Type = o.OnlineAccounts_v1_0.OnlineAccountTypes_v1_0.service,
                                        Account = o.OnlineAccounts_v1_0.username
                                    }).ToList(),
                                }
                            }).ToList();
                return keys;
            }
        }

        public virtual string GetMachineAuthorization(Guid applicationInstance, string machineKey, string requestUrl)
        {
             using (var Entities = Repository)
            {
                string token = String.Empty;
                var instance = Entities.ApplicationInstance_v1_0.Where(a => a.instanceUID == applicationInstance);
                if (instance.Count() == 1)
                {
                    var keys = Entities.MachineKeys_v1_0.Where(m => m.machineKey == machineKey);
                    if (keys.Count() == 1)
                    {
                        var match = instance.Single().ApplicationInstanceMachineKeys_v1_0.Where(k => k.authorized && k.machineKeyId == keys.Single().id);
                        token = match.Count() == 1 ? Licensing.Utility.GenerateLicensingToken(applicationInstance.ToString(), machineKey.Replace("-", ""), new TimeSpan(0, 5, 0)) : String.Empty;
                    }
                }
 
                Entities.WebEvent_v1_0.AddObject(new WebEvent_v1_0()
                {
                    EventId = Guid.NewGuid().ToString().Replace("-",""),
                    RequestUrl = requestUrl,
                    ApplicationPath = AppDomain.CurrentDomain.BaseDirectory,
                    ApplicationVirtualPath = applicationInstance==null?"":applicationInstance.ToString(),
                    EventTime = DateTime.Now,
                    EventTimeUtc = DateTime.Now.ToUniversalTime(),
                    EventType = "MachineAuthorization",
                    EventCode = 4,
                    EventDetailCode = String.IsNullOrEmpty(token)?1:2,
                    EventOccurrence = 1,
                    EventSequence = 1,
                    MachineName = machineKey,
                    Details = "Machine=" + Environment.MachineName + ";User=" + Environment.UserName + ";Assembly=" + this.GetType().AssemblyQualifiedName,
                    //+ System.Reflection.Assembly.GetEntryAssembly().FullName,
                    Message = token
                });
                Entities.SaveChanges();

                 return token;
        
            }
        }

        public virtual void AuthorizeMachine(Guid applicationInstance, string machineKey, string email)
        {
            if (!((MembershipProvider)Membership.Provider).IsValidApplicationMember(applicationInstance, email))
            {
                throw new KeyNotFoundException(email);
            }

            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var contacts = Entities.ContactEmails_v1_0.Where(e => e.Email_v1_0.formattedEmailAddress == email);
                if (contacts.Count() == 0)
                {
                    throw new KeyNotFoundException(email);
                }
                else
                {
                    Contacts_v1_0 contact = contacts.Single().Contacts_v1_0;
                    var instances = Entities.ApplicationInstanceMachineKeys_v1_0.Where(i => i.MachineKeys_v1_0.machineKey == machineKey && i.ApplicationInstance_v1_0.instanceUID == applicationInstance);
                    if (instances.Count() == 0)
                    {
                         var machines = Entities.MachineKeys_v1_0.Where(m => m.machineKey == machineKey);
                         if (machines.Count() == 0)
                         {
                             Entities.ApplicationInstanceMachineKeys_v1_0.AddObject(new ApplicationInstanceMachineKeys_v1_0()
                             {
                                 ApplicationInstance_v1_0 = Entities.ApplicationInstance_v1_0.Single(a => a.instanceUID == applicationInstance),
                                 MachineKeys_v1_0 = new MachineKeys_v1_0() { contactId = contact.id, machineKey = machineKey, createDate = DateTime.Now, machineDescription = (contact.lastName??"") + "," + (contact.firstName??"") },
                                 authorized = true,
                                 authorizedBy = authority,
                                 authorizedDate = DateTime.Now,
                                 createDate = DateTime.Now,
                                 createdBy = authority
                             });
                         }
                         else
                         {
                             var machine = machines.Single();
                             if (machine.contactId != contact.id)
                             {
                                 throw new InvalidOperationException("Machine is not asscoiated with the contact");
                             }
                             Entities.ApplicationInstanceMachineKeys_v1_0.AddObject(new ApplicationInstanceMachineKeys_v1_0()
                             {
                                 ApplicationInstance_v1_0 = Entities.ApplicationInstance_v1_0.Single(a => a.instanceUID == applicationInstance),
                                 machineKeyId = machine.id,
                                 authorized = true,
                                 authorizedBy = authority,
                                 authorizedDate = DateTime.Now,
                                 createDate = DateTime.Now,
                                 createdBy = authority
                             });
                         
                         }
                        Entities.SaveChanges();

                    }
                    else
                    {
                        var instance = instances.Single();
                        instance.authorized = true;
                        instance.lastModifiedBy = authority;
                        instance.lastModifiedDate = DateTime.Now;
                        Entities.SaveChanges();
                    }
                }
            }
        }

        public virtual void AuthorizeMachine(Guid applicationInstance, MachineKey machineKey)
        {
            if (!((MembershipProvider)Membership.Provider).IsValidApplicationMember(applicationInstance, machineKey.Email))
            {
                throw new KeyNotFoundException(machineKey.Email);
            }

            using (var Entities = Repository)
            {
                //Entities.Users_v1_0.Where(u => u.userName == System.Threading.Thread.CurrentPrincipal.Identity.Name).Single().contactId;
                var authority = GetCurrentAuthority(applicationInstance);
                var contacts = Entities.ContactEmails_v1_0.Where(e => e.Email_v1_0.formattedEmailAddress == machineKey.Email);
                if (contacts.Count() == 0)
                {
                    throw new KeyNotFoundException(machineKey.Email);
                }
                else
                {
                    Contacts_v1_0 contact = contacts.Single().Contacts_v1_0;

                    if (String.IsNullOrEmpty(contact.firstName))
                    {
                        contact.firstName = machineKey.Contact.FirstName;
                    }

                    if (String.IsNullOrEmpty(contact.lastName))
                    {
                        contact.lastName = machineKey.Contact.LastName;
                    }

                    var instances = Entities.ApplicationInstanceMachineKeys_v1_0.Where(i => i.MachineKeys_v1_0.machineKey == machineKey.Key && i.ApplicationInstance_v1_0.instanceUID == applicationInstance);
                    if (instances.Count() == 0)
                    {
                         var machines = Entities.MachineKeys_v1_0.Where(m => m.machineKey == machineKey.Key);
                         if (machines.Count() == 0)
                         {
                             Entities.ApplicationInstanceMachineKeys_v1_0.AddObject(new ApplicationInstanceMachineKeys_v1_0()
                             {
                                 ApplicationInstance_v1_0 = Entities.ApplicationInstance_v1_0.Single(a => a.instanceUID == applicationInstance),
                                 MachineKeys_v1_0 = new MachineKeys_v1_0() { contactId = contact.id, machineKey = machineKey.Key, createDate  = DateTime.Now, machineDescription = (contact.lastName??"") + "," + (contact.firstName??"") },
                                 authorized = true,
                                 authorizedBy = authority,
                                 authorizedDate = DateTime.Now,
                                 createDate = DateTime.Now,
                                 createdBy = authority
                             });
                         }
                         else
                         {
                             var machine = machines.Single();
                             if (machine.contactId != contact.id)
                             {
                                 throw new InvalidOperationException("Machine is not asscoiated with the contact");
                             }
                             Entities.ApplicationInstanceMachineKeys_v1_0.AddObject(new ApplicationInstanceMachineKeys_v1_0()
                             {
                                 ApplicationInstance_v1_0 = Entities.ApplicationInstance_v1_0.Single(a => a.instanceUID == applicationInstance),
                                 machineKeyId = machine.id,
                                 authorized = true,
                                 authorizedBy = authority,
                                 authorizedDate = DateTime.Now,
                                 createDate = DateTime.Now,
                                 createdBy = authority
                             });
                         }
                        Entities.SaveChanges();

                    }
                    else
                    {
                        var instance = instances.Single();
                        instance.authorized = true;
                        instance.lastModifiedBy = authority;
                        instance.lastModifiedDate = DateTime.Now;
                        Entities.SaveChanges();
                    }
                }
            }
        }

        public virtual void RevokeMachine(Guid applicationInstance, string machineKey)
        {
            using (var Entities = Repository)
            {
                var authority = GetCurrentAuthority(applicationInstance);
                var instances = Entities.ApplicationInstanceMachineKeys_v1_0.Where(i => i.MachineKeys_v1_0.machineKey == machineKey && i.ApplicationInstance_v1_0.instanceUID == applicationInstance);
                if (instances.Count() != 1)
                {
                    throw new KeyNotFoundException(machineKey);
                }

                var instance = instances.Single();
                instance.authorized = false;
                instance.lastModifiedBy = authority;
                instance.lastModifiedDate = DateTime.Now;
                Entities.SaveChanges();
            }
        }

        public virtual string CreateSubscriptionToken(Guid applicationInstance, string email)
        {
            return new ValidationToken(email) { Memento = applicationInstance.ToString() };
        }

        public virtual void Subscribe(Guid applicationInstance, string email)
        {
            var authority = GetCurrentAuthority(applicationInstance);
            using (var Entities = Repository)
            {
                var users = Entities.Users_v1_0.Where(u => u.email.Equals(email, StringComparison.InvariantCultureIgnoreCase));
                switch (users.Count())
                {
                    case 0:
                        throw new KeyNotFoundException(email);
                    case 1:
                        var user = users.Single();
                        var application = Entities
                                        .ApplicationInstance_v1_0
                                        .SingleOrDefault(i => i.instanceUID == applicationInstance);
                        var members = application.Membership_v1_0.Where(m => m.userId == user.id);
                        if (members.Count() == 0)
                        {
                            application.Membership_v1_0.Add(new Membership_v1_0()
                            {
                                userId = user.id,
                                privilegeId = 3,
                                isApproved = true,
                                enabled = true,
                                createdBy = authority,
                                createDate = DateTime.Now,
                                approvedBy = authority,
                                approvedDate = DateTime.Now,
                            });
                           
                        }
                        else
                        {
                            members.Single().isApproved = true;
                            //members.Single().enabled = true;
                            members.Single().privilegeId = 3;
                            members.Single().lastModifiedBy = authority;
                            members.Single().lastModifiedDate = DateTime.Now;
                        }
                        Entities.SaveChanges();
                        break;
                    default:
                        throw new Exception("duplicate keys found for " + email);
                }
            }
        }

        public virtual void ValidateSubscriptionToken(string email, ValidationToken token, out ValidationTokenStatus status)
        {
            using (var Entities = Repository)
            {
                var user = Entities.Users_v1_0.Where(u => u.email.Equals(email, StringComparison.InvariantCultureIgnoreCase)).SingleOrDefault();
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

        public void Initialize(string name, System.Collections.Specialized.NameValueCollection config)
        {
            string connectionStringName = config["connectionStringName"];
            _connectionString = WebConfigurationManager.ConnectionStrings[connectionStringName].ConnectionString;
            Settings = config;
        }

        private int GetCurrentAuthority(Guid applicationInstance)
        {
            using (var Entities = Repository)
            {
                var application = Entities.
                    ApplicationInstance_v1_0
                    .SingleOrDefault(i => i.instanceUID == applicationInstance);
                if (string.IsNullOrEmpty(System.Threading.Thread.CurrentPrincipal.Identity.Name) || !(System.Threading.Thread.CurrentPrincipal.IsInRole("Owner") || System.Threading.Thread.CurrentPrincipal.IsInRole("Administrator")))
                {
                    return Entities
                        .Membership_v1_0
                        .Where(m => m.applicationInstanceId == application.id && m.privilegeId == 1)
                        .Select(m => m.Users_v1_0)
                        .FirstOrDefault().id;
                }
                else
                {
                    return Entities.Users_v1_0
                        .Where(u => u.email.Equals(System.Threading.Thread.CurrentPrincipal.Identity.Name, StringComparison.InvariantCultureIgnoreCase))
                        .SingleOrDefault().id;
                }
            }
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

        [Format("EMAIL", @"[A-Za-z0-9._%+-]+@[A-Za-z0-9.-]+\.[A-Za-z]{2,4}", TokenType.REGEXP)]
        [Format("MACHINEID", @"\\([0-9A-Za-z]{4})-([0-9A-Za-z]{4})-([0-9A-Za-z]{4})-([0-9A-Za-z]{4})-([0-9A-Za-z]{4})", TokenType.REGEXP)]
        [Format("APPLICATIONID", @"\{([0-9A-Za-z]{8})-([0-9A-Za-z]{4})-([0-9A-Za-z]{4})-([0-9A-Za-z]{4})-([0-9A-Za-z]{12})\}", TokenType.REGEXP)]
        public class EMailToken : Token
        {
            public static string ExtractMachineId(string value)
            {
                return TextParser.Parse((string)value, new EMailToken()).Where(t => t.Name == "MACHINEID").Single().Items.Single().ToString().Trim('\\');
            }

            public static string ExtractApplicationId(string value)
            {
                return TextParser.Parse((string)value, new EMailToken()).Where(t => t.Name == "APPLICATIONID").Single().Items.Single().ToString().Trim('\\');
            }
        }

    }
}
