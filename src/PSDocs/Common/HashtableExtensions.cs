
using System.Collections;
using System.Diagnostics;

namespace PSDocs.Common
{
    internal static class HashtableExtensions
    {
        [DebuggerStepThrough]
        public static void AddUnique(this Hashtable hashtable, Hashtable values)
        {
            if (values == null)
                return;

            foreach (var key in values.Keys)
                if (!hashtable.ContainsKey(key))
                    hashtable.Add(key, values[key]);
        }
    }
}
