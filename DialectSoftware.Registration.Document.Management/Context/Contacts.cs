using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web.Http;

namespace DialectSoftware.Registration
{
    public class Contacts:IContacts
    {
        string _db;
        SharpCouch.DB _couch;

        public Contacts(SharpCouch.DB couch, string db)
        {
            _db = db;
            _couch = couch;
        }

        public Contact this[string id]
        {
            get 
            {
                var doc = _couch.GetDocument(_db, id);
                LitJson.JsonMapper.RegisterImporter<string, Guid>((s) => { return Guid.Parse(s); });
                return LitJson.JsonMapper.ToObject<Contact>(doc);
            }
            set 
            {
                if(this[id]._rev == value._rev && id == value._id)
                    Add(value);
                else
                    throw new HttpResponseException(HttpStatusCode.Conflict); 
            }
        }

        public void Add(Contact contact)
        {
            LitJson.JsonMapper.RegisterExporter<Guid>(new LitJson.ExporterFunc<Guid>((g, w) =>
            {
                w.Write(g.ToString());
            }));

            _couch.CreateDocument(_db, LitJson.JsonMapper.ToJson(contact));
        }

        public void Remove(Contact contact)
        {
           Remove(contact._id);
        }

        public void Remove(string id)
        {
            string rev = _couch.GetRevision(_db, id);
            Remove(id, rev);
        }

        public void Remove(string id, string rev)
        {
            _couch.DeleteDocument(_db, id, rev);
        }

        public IEnumerator<Contact> GetEnumerator()
        {
            int total = 0;
            var docs = _couch.GetAllDocuments(_db, "contacts", "contacts_by_id", null, 2, out total, true);
            LitJson.JsonMapper.RegisterImporter<string, Guid>((s) => { return Guid.Parse(s); });
            IEnumerable<Contact> contacts = docs.Select(doc => { var contact = LitJson.JsonMapper.ToObject<Contact>(doc.Document); /*contact._rev = doc.Revision;*/ return contact; });
            yield return contacts.First();
            total -= 1;
            while (total > 0)
            {
                int index = 0;
                docs = _couch.GetAllDocuments(_db, "contacts", "contacts_by_id", docs.ElementAt(1).Key, 2, out index, true);
                contacts = docs.Select(doc => { var contact = LitJson.JsonMapper.ToObject<Contact>(doc.Document); /*contact._rev = doc.Revision;*/ return contact; });
                yield return contacts.First();
                total -= 1;
            }
        }

        public IEnumerable<Contact> Next(string id, int count, out int totalRecords)
        {
            LitJson.JsonMapper.RegisterImporter<string, Guid>((s) => { return Guid.Parse(s); });
            if(id == null)
                 return _couch.GetAllDocuments(_db, "contacts", "contacts_by_id", id, count, out totalRecords, true).Skip(0).Select(doc => { var contact = LitJson.JsonMapper.ToObject<Contact>(doc.Document); /*contact._rev = doc.Revision;*/ return contact; });
            else
                return _couch.GetAllDocuments(_db, "contacts", "contacts_by_id", id, count + 1, out totalRecords, true).Skip(1).Select(doc => { var contact = LitJson.JsonMapper.ToObject<Contact>(doc.Document); /*contact._rev = doc.Revision;*/ return contact; });
      
        }

        public static Contacts Connect(SharpCouch.DB couch, string db)
        {
            return new Contacts(couch, db);
        }

        public Contact GetByEmailAddress(string email)
        {
            var docs = _couch.GetAllDocuments(_db, "contacts", "contacts_by_email", email, true);
            LitJson.JsonMapper.RegisterImporter<string, Guid>((s) => { return Guid.Parse(s); });
            return docs.Select(doc => { var contact = LitJson.JsonMapper.ToObject<Contact>(doc.Document); /*contact._rev = doc.Revision;*/ return contact; }).DefaultIfEmpty(null).SingleOrDefault();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public void Create()
        {
            string view = null;
            var sys = Sys.Connect(_couch, _db);
            try
            {
                view = sys.GetView("contacts");
            }
            finally
            {
                if (view == null)
                {
                    sys.CreateView("contacts",
                                   "{\"_id\": \"_design/contacts\",\"language\": \"javascript\",\"views\": {\"contacts_by_id\": {\"map\": \"function(doc) { if ('Contact' == doc.type ) {  emit(doc._id,{rev:doc._rev}); }}\"},\"contacts_by_email\": {\"map\": \"function(doc) { if ('Contact' == doc.type ) { for (var i in doc.emailAddresses) { emit(doc.emailAddresses[i].formattedEmailAddress,{rev:doc._rev});}}}\"}}}");
                }
            }
        }

    }
}
