// SharpCouch - a simple wrapper class for the CouchDB HTTP API
// Copyright 2007 Ciaran Gultnieks
//
// This program is free software; you can redistribute it and/or
// modify it under the terms of the GNU General Public License
// as published by the Free Software Foundation; either version 2
// of the License, or (at your option) any later version.
//
// This program is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with this program; if not, write to the Free Software
// Foundation, Inc., 51 Franklin Street, Fifth Floor, Boston, MA  02110-1301, USA.

using System;
using System.IO;
using System.Net;
using System.Web;
using System.Text;
using System.Collections.Generic;
using LitJson;
using System.Linq;

namespace SharpCouch
{

    /// <summary>
    /// A simple wrapper class for the CouchDB HTTP API. No
    /// initialisation is necessary, just create an instance and
    /// call the appropriate methods to interact with CouchDB.
    /// All methods throw exceptions when things go wrong.
    /// </summary>
    public class DB
    {
        string _username;
        string _password;
        string _server;

        public DB(string server)
        {
            _server = server;
        }

        public DB(string server, string username, string password):this(server)
        {
            _username = username;
            _password = password;
        }

        /// <summary>
        /// Get a list of database on the server.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <returns>A string array containing the database names
        /// </returns>
        public string[] GetDatabases()
        {
            string result = DoRequest(_server + "/_all_dbs", "GET");

            List<string> list = new List<string>();
            if (result != "[]")
            {
                JsonData d = JsonMapper.ToObject(result);
                foreach (JsonData db in d)
                    list.Add(db.ToString());
            }
            return (list.ToArray());
        }

        /// <summary>
        /// Get the document count for the given database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <returns>The number of documents in the database</returns>
        public int CountDocuments(string db)
        {
            // Get information about the database...
            string result = DoRequest(_server + "/" + db, "GET");

            // The document count is a field within...
            JsonData d = JsonMapper.ToObject(result);
            int count = int.Parse(d["doc_count"].ToString());
            return count;
        }

        /// <summary>
        /// Get information on all the documents in the given database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>
        public DocInfo[] GetAllDocuments(string db)
        {
            string result = DoRequest(_server + "/" + db + "/_all_docs", "GET");

            List<DocInfo> list = new List<DocInfo>();

            JsonData d = JsonMapper.ToObject(result);
            foreach (JsonData row in d["rows"])
            {
                DocInfo doc = new DocInfo();
                doc.Id = row["id"].ToString();
                doc.Key = row["key"].ToJson();
                doc.Revision = (row["value"])["rev"].ToString();
                list.Add(doc);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get information on all the documents in the given database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>
        public DocInfo[] GetAllDocuments(string db, string id, string view, string key, bool include_doc = false)
        {
            string result = DoRequest(_server + "/" + db + "/_design/" + id + "/_view/" + view + "?include_docs=" + include_doc.ToString().ToLower() + "&key=%22" + key + "%22", "GET");

            List<DocInfo> list = new List<DocInfo>();

            JsonData d = JsonMapper.ToObject(result);
            foreach (JsonData row in d["rows"])
            {
                DocInfo doc = new DocInfo();
                doc.Id = row["id"].ToString();
                doc.Key = row["key"].ToJson();
                doc.Revision = (row["value"])["rev"].ToString();
                if (include_doc)
                    doc.Document = row["doc"].ToJson();
                list.Add(doc);
            }
            return list.ToArray();
        }


        /// <summary>
        /// Get information on all the documents in the given database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>
        public DocInfo[] GetAllDocuments(string db, string id, string view, string[] keys, bool include_doc = false)
        {
            string key = String.Format("[{0}]", string.Join(",", keys.Select(k => String.Format("\"{0}\"", k)).ToArray()));
            string result = DoRequest(_server + "/" + db + "/_design/" + id + "/_view/" + view + "?include_docs=" + include_doc.ToString().ToLower() + "&key=" + key, "GET");

            List<DocInfo> list = new List<DocInfo>();

            JsonData d = JsonMapper.ToObject(result);
            foreach (JsonData row in d["rows"])
            {
                DocInfo doc = new DocInfo();
                doc.Id = row["id"].ToString();
                doc.Key = row["key"].ToJson();
                doc.Revision = (row["value"])["rev"].ToString();
                if (include_doc)
                    doc.Document = row["doc"].ToJson();
                list.Add(doc);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Get information on all the documents in the given database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <returns>An array of DocInfo instances</returns>
        public DocInfo[] GetAllDocuments(string db, string id, string view, string startkey, int count, out int total, bool include_doc=false)
        {
            string result = DoRequest(_server + "/" + db + "/_design/" + id + "/_view/" + view + "?include_docs=" + include_doc.ToString().ToLower() + "&limit=" + count.ToString() + (startkey == null ? "" : "&startkey=%22" + startkey.Trim(new[] { '"' }) + "%22"), "GET");

            List<DocInfo> list = new List<DocInfo>();

            JsonData d = JsonMapper.ToObject(result);
            total = (int)d["total_rows"];
            foreach (JsonData row in d["rows"])
            {
                DocInfo doc = new DocInfo();
                doc.Id = row["id"].ToString();
                doc.Key = row["key"].ToJson();
                doc.Revision = (row["value"])["rev"].ToString();
                if (include_doc)
                    doc.Document = row["doc"].ToJson();
                list.Add(doc);
            }
            return list.ToArray();
        }

        /// <summary>
        /// Create a new database.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        public void CreateDatabase(string db)
        {
            string result = DoRequest(_server + "/" + db, "PUT");
            if (result != "{\"ok\":true}\n")
                throw new ApplicationException("Failed to create database: " + result);
        }

        /// <summary>
        /// Delete a database
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The name of the database to delete</param>
        public void DeleteDatabase(string db)
        {
            string result = DoRequest(_server + "/" + db, "DELETE");
            if (result != "{\"ok\":true}\n")
                throw new ApplicationException("Failed to delete database: " + result);
        }

        /// <summary>
        /// Execute a temporary view and return the results.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="map">The javascript map function</param>
        /// <param name="reduce">The javascript reduce function or
        /// null if not required</param>
        /// <param name="startkey">The startkey or null not to use</param>
        /// <param name="endkey">The endkey or null not to use</param>
        /// <returns>The result (JSON format)</returns>
        public string ExecTempView(string db, string map, string reduce, string startkey, string endkey, int skip=0, int limit=0)
        {
            // Generate the JSON view definition from the supplied
            // map and optional reduce functions...
            string viewdef = "{ \"map\":\"" + map + "\"";
            if (reduce != null)
                viewdef += ",\"reduce\":\"" + reduce + "\"";
            viewdef += "}";

            string url = _server + "/" + db + "/_temp_view";
            if (startkey != null)
            {
                url += "?startkey=" + HttpUtility.UrlEncode(startkey);
            }
            if (endkey != null)
            {
                if (startkey == null) url += "?"; else url += "&";
                url += "endkey=" + HttpUtility.UrlEncode(endkey);
            }
            if (skip != 0)
            {
                if (startkey == null) url += "?"; else url += "&";
                url += "skip=" + skip.ToString();
            }
            if (limit != 0)
            {
                if (startkey == null) url += "?"; else url += "&";
                url += "limit=" + limit.ToString();
            }
            return DoRequest(url, "POST", viewdef, "application/json");
        }

        /// <summary>
        /// Execute a temporary view and return the results.
        /// </summary>
        /// <param name=”server”>The server URL</param>
        /// <param name=”db”>The database name</param>
        /// <param name=”map”>The javascript map function</param>
        /// <param name=”reduce”>The javascript reduce function or
        /// null if not required</param>
        /// <param name=”startkey”>The startkey or null not to use</param>
        /// <param name=”endkey”>The endkey or null not to use</param>
        /// <returns>The result (JSON format)</returns>
        public string ExecTempView(string db,string map,string reduce,string startkey,string endkey, bool Group, int Limit,int Skip)
        {
            // Generate the JSON view definition from the supplied
            // map and optional reduce functions…
            string viewdef="{ \"map\":\""+map+"\"";
            if(reduce!=null)
                viewdef+=",\"reduce\":\""+reduce+"\"";
            viewdef+="}";

            string url=_server+"/"+db+"/_temp_view";
            if(startkey!=null)
            {
                url+="?startkey="+HttpUtility.UrlEncode(startkey);
            }
            if(endkey!=null)
            {
                if(startkey==null) url+="?"; else url+="&";
                    url+="endkey="+HttpUtility.UrlEncode(endkey);
            }
            if (Group)
            {
                url += "?group=true";
            }
            if (Limit > 0)
            {
                if (Group) url += "&"; else url += "?";
                    url += "limit=" + Limit;
            }
            if (Skip > 0)
            {
                if (Group || Limit>0) url += "&"; else url += "?";
                    url += "skip=" + Skip;
            }

            return DoRequest(url,"POST",viewdef,"application/json");
        }

        /// <summary>
        /// Create a new document. If the document has no ID field,
        /// it will be assigned one by the server.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="content">The document contents (JSON).</param>
        public void CreateDocument(string db, string content)
        {
            DoRequest(_server + "/" + db, "POST", content, "application/json");
        }

        /// <summary>
        /// Create a new document. If the document has no ID field,
        /// it will be assigned one by the server.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="content">The document contents (JSON).</param>
        //TODO:VERIFY
        public void CreateDesignDocument(string db, string content)
        {
            DoRequest(_server + "/" + db, "PUT", content, "application/json");
        }

        /// <summary>
        /// Get a document.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="docid">The document ID.</param>
        /// <returns>The document contents (JSON)</returns>
        public string GetDocument(string db, string docid)
        {
            return DoRequest(_server + "/" + db + "/" + docid, "GET");
        }

        /// <summary>
        /// Get a document.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="docid">The document ID.</param>
        /// <param name="startkey">The startkey or null not to use</param>
        /// <param name="endkey">The endkey or null not to use</param>
        /// <returns>The document contents (JSON)</returns>
        public string GetDocument(string db, string docid, string startkey, string endkey)
        {
            string url = _server + "/" + db + "/" + docid;
            if (startkey != null)
            {
                url += "?startkey=" + HttpUtility.UrlEncode(startkey);
            }
            if (endkey != null)
            {
                if (startkey == null) url += "?"; else url += "&";
                url += "endkey=" + HttpUtility.UrlEncode(endkey);
            }
            return DoRequest(url, "GET");
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="db"></param>
        /// <param name="docid"></param>
        /// <returns></returns>
        public string GetRevision(string db, string docid)
        {
            //HEAD /somedatabase/some_doc_id HTTP/1.0
            string url = _server + "/" + db + "/" + docid;
            return DoRequest(url, "HEAD");
        }

        /// <summary>
        /// Delete a document.
        /// </summary>
        /// <param name="server">The server URL</param>
        /// <param name="db">The database name</param>
        /// <param name="docid">The document ID.</param>
        public void DeleteDocument(string db, string docid, string rev)
        {
            DoRequest(_server + "/" + db + "/" + docid + "?rev=" + rev, "DELETE");
        }

        /// <summary>
        /// Internal helper to make an HTTP request and return the
        /// response. Throws an exception in the event of any kind
        /// of failure. Overloaded - use the other version if you
        /// need to post data with the request.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="method">The method, e.g. "GET"</param>
        /// <returns>The server's response</returns>
        private string DoRequest(string url, string method)
        {
            return DoRequest(url, method, null, null);
        }

        /// <summary>
        /// Internal helper to make an HTTP request and return the
        /// response. Throws an exception in the event of any kind
        /// of failure. Overloaded - use the other version if no
        /// post data is required.
        /// </summary>
        /// <param name="url">The URL</param>
        /// <param name="method">The method, e.g. "GET"</param>
        /// <param name="postdata">Data to be posted with the request,
        /// or null if not required.</param>
        /// <param name="contenttype">The content type to send, or null
        /// if not required.</param>
        /// <returns>The server's response</returns>
        private string DoRequest(string url, string method, string postdata, string contenttype)
        {
            HttpWebRequest request = WebRequest.Create(url) as HttpWebRequest;
            request.Headers.Add("Accept-Charset", "utf-8");
            request.Headers.Add("Accept-Language", "en-us");
            request.ContentType = "application/json";
            request.Method = method;
            // Yuk - set an infinite timeout on this for now, because
            // executing a temporary view (for example) can take a very
            // long time...
            request.Timeout = System.Threading.Timeout.Infinite;

            if (_username != null && _password != null)
            {
                //important
                request.Headers.Clear();
                request.KeepAlive = true;
                // Deal with Authorization Header
                string authValue = "Basic ";
                string userNAndPassword = _username + ":" + _password;
                // Base64 encode
                string b64 = System.Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes(userNAndPassword));
                authValue = authValue + b64;
                request.Headers.Add("Authorization", authValue);
            }

            if (contenttype != null)
                request.ContentType = contenttype;

            if (postdata != null)
            {
                byte[] bytes = UTF8Encoding.UTF8.GetBytes(postdata.ToString());
                request.ContentLength = bytes.Length;
                using (Stream ps = request.GetRequestStream())
                {
                    ps.Write(bytes, 0, bytes.Length);
                }
            }

            HttpWebResponse resp = request.GetResponse() as HttpWebResponse;
            
            string result;
            if(method.Equals("HEAD",StringComparison.InvariantCultureIgnoreCase))
            {
                result = resp.Headers.Get("ETag").Trim(new[] { '"' });
            }
            else
            {
                using (StreamReader reader = new StreamReader(resp.GetResponseStream()))
                {
                    result = reader.ReadToEnd();
                }
            }
            return result;
        }

    }
}