using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DialectSoftware.Registration
{
    public class Sys
    {
        string _db;
        SharpCouch.DB _couch;

        public Sys(SharpCouch.DB couch, string db)
        {
            _db = db;
            _couch = couch;
        }

        public string GetView(string id)
        {
            return _couch.GetDocument(_db,  "/_design/" + id);
        }

        public void CreateView(string id, string view)
        {
            _couch.CreateDesignDocument(_db + "/_design/" + id, view);
        }

        public void DeleteView(string id, string view)
        {
            string rev = _couch.GetRevision(_db, "_design/" + id);
            _couch.DeleteDocument(_db, "_design/" + id, rev);
        
        }
        public static Sys Connect(SharpCouch.DB couch, string db)
        {
            return new Sys(couch, db);
        }
    }
}
