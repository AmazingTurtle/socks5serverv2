using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Linq
{
    public static class Extension
    {
        public static void ForEach<T>(this IEnumerable<T> enumeration, Action<T> action)
        {
            foreach (T item in enumeration)
            {
                action(item);
            }
        }
        public static IEnumerable<T2> ForEach<T1, T2>(this IEnumerable<T1> enumeration, Func<T1, T2> action)
        {
            foreach (T1 item in enumeration)
            {
                yield return action(item);
            }
        }

    }
}

namespace System
{

    public static class Extension
    {
        public static long PeekBytes(this System.IO.Stream stream)
        {
            return stream.Length - stream.Position;
        }

        public static bool HasRawHandler(this Socks5S.Plugin.IPlugin plugin)
        {
            return plugin.RawHandler != null;
        }

        public static bool HasStateDependentHandler(this Socks5S.Plugin.IPlugin plugin)
        {
            return plugin.StateDependentHandler != null;
        }

    }

}
