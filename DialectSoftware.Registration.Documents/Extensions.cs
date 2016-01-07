using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DialectSoftware.Registration
{
    public static class Extensions
    {
        public static StringBuilder AppendOnly(this StringBuilder sb, string value)
        {
            if (value != null)
                sb.AppendLine(value);
            return sb;
        }
    }
}
