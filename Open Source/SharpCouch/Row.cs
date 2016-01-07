using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpCouch
{
    public class Row<T>
    {
        public Row()
        { 
        
        }
        public string id { get; set; }
        public string key { get; set; }
        public T value { get; set; }
    }
}
