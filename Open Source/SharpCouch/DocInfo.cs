using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpCouch
{
    /// <summary>
    /// Used to return metadata about a document.
    /// </summary>
    public class DocInfo
    {
        public string Id;
        public string Key;
        public string Revision;
        public string Document;
    }
}
