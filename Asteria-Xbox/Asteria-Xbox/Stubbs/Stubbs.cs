using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;

// to get compiler to shut up
namespace System.Threading.Tasks
{
}

namespace HD
{
    public delegate TResult Func<in T1, in T2, in T3, in T4, in T5, out TResult>(T1 arg1, T2 arg2, T3 arg3, T4 arg4, T5 arg5);

    public static class Stubbs
    {
        public static bool IsNullOrWhitespace(String value)
        {
            if (value == null)
                return true;
            if (value.Length == 0 || value.Trim().Length == 0)
                return true;
            return false;
        }
    }
}