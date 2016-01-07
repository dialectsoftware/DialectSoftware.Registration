using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SharpCouch
{
    public class Rows<T>:IEnumerable<T>
    {
        public Rows()
        { 
        }

        public int total_rows { get; set; }
        public int offset { get; set; }
        public Row<T>[] rows { get; set; }

        public Row<T> this[int i]
        {
            get 
            {
                return rows[i];
            }
        }

        public IEnumerator<T>  GetEnumerator()
        {
 	       foreach(Row<T> row in rows)
           {
               yield return row.value;
           }
        }

        System.Collections.IEnumerator  System.Collections.IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
